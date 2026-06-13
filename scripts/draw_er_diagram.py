# /// script
# requires-python = ">=3.11"
# dependencies = [
#     "python-tds>=1.16",
#     "networkx>=3.2",
#     "matplotlib>=3.8",
# ]
# ///
"""Draw entity-relationship diagrams for the AeroMes database.

Reads live schema metadata (tables, columns, PKs, FKs) from the SQL Server
instance the API migrated, then renders:

  - overview.png            — every table, color-coded by bounded context
  - context-<name>.png      — per-context detail with PK/FK columns listed

Run:  uv run scripts/draw_er_diagram.py
"""

from __future__ import annotations

import os
import re
from collections import defaultdict
from dataclasses import dataclass, field

import matplotlib

matplotlib.use("Agg")

import matplotlib.patches as mpatches
import matplotlib.pyplot as plt
import networkx as nx
import pytds

# ---------------------------------------------------------------- connection

SERVER = os.environ.get("AEROMES_DB_SERVER", "localhost")
PORT = int(os.environ.get("AEROMES_DB_PORT", "1433"))
DATABASE = os.environ.get("AEROMES_DB_NAME", "AeroMesDb_works2")
USER = os.environ.get("AEROMES_DB_USER", "sa")
PASSWORD = os.environ.get("AEROMES_DB_PASSWORD", "Devduide@123")

OUT_DIR = os.path.join(os.path.dirname(__file__), "..", "docs", "er-diagrams")

# DB schema -> bounded context (the EF model maps each context to a schema).
SCHEMA_CONTEXTS: dict[str, str] = {
    "auth": "Auth",
    "integration": "Integration",
    "master": "Master",
    "prod": "Production",
    "qual": "Quality",
    "settings": "Settings",
}

CONTEXT_COLORS = {
    "Auth": "#e15759",
    "Identity": "#f28e2b",
    "Integration": "#76b7b2",
    "Master": "#4e79a7",
    "Production": "#59a14f",
    "Quality": "#b07aa1",
    "Settings": "#9c755f",
    "Other": "#bab0ac",
}


@dataclass
class Table:
    name: str
    schema: str = "dbo"
    context: str = "Other"
    columns: list[tuple[str, str, bool]] = field(default_factory=list)  # (name, type, nullable)
    pk_columns: set[str] = field(default_factory=set)
    fk_columns: set[str] = field(default_factory=set)


def classify(schema: str, table: str) -> str:
    if table.startswith("AspNet"):
        return "Identity"
    return SCHEMA_CONTEXTS.get(schema, "Other")


def fetch_schema() -> tuple[dict[str, Table], list[tuple[str, str, str, str]]]:
    conn = pytds.connect(
        server=SERVER, port=PORT, database=DATABASE,
        user=USER, password=PASSWORD, autocommit=True,
    )
    cur = conn.cursor()

    cur.execute("""
        SELECT s.name, t.name, c.name, ty.name, c.is_nullable
        FROM sys.tables t
        JOIN sys.schemas s ON s.schema_id = t.schema_id
        JOIN sys.columns c ON c.object_id = t.object_id
        JOIN sys.types ty ON ty.user_type_id = c.user_type_id
        WHERE t.is_ms_shipped = 0
        ORDER BY t.name, c.column_id
    """)
    tables: dict[str, Table] = {}
    for sname, tname, cname, ctype, nullable in cur.fetchall():
        if tname.startswith("__"):  # __EFMigrationsHistory
            continue
        tables.setdefault(tname, Table(tname, sname)).columns.append(
            (cname, ctype, bool(nullable))
        )

    cur.execute("""
        SELECT t.name, c.name
        FROM sys.tables t
        JOIN sys.indexes i ON i.object_id = t.object_id AND i.is_primary_key = 1
        JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = ic.column_id
    """)
    for tname, cname in cur.fetchall():
        if tname in tables:
            tables[tname].pk_columns.add(cname)

    cur.execute("""
        SELECT tp.name, cp.name, tr.name, cr.name
        FROM sys.foreign_key_columns fkc
        JOIN sys.tables tp ON tp.object_id = fkc.parent_object_id
        JOIN sys.columns cp ON cp.object_id = fkc.parent_object_id AND cp.column_id = fkc.parent_column_id
        JOIN sys.tables tr ON tr.object_id = fkc.referenced_object_id
        JOIN sys.columns cr ON cr.object_id = fkc.referenced_object_id AND cr.column_id = fkc.referenced_column_id
    """)
    fks: list[tuple[str, str, str, str]] = []
    for ptable, pcol, rtable, rcol in cur.fetchall():
        if ptable in tables and rtable in tables:
            fks.append((ptable, pcol, rtable, rcol))
            tables[ptable].fk_columns.add(pcol)

    conn.close()

    for t in tables.values():
        t.context = classify(t.schema, t.name)
    return tables, fks


# ------------------------------------------------------------------ drawing

def draw_overview(tables: dict[str, Table], fks: list, path: str) -> None:
    g = nx.DiGraph()
    for t in tables.values():
        g.add_node(t.name, context=t.context)
    for ptable, _, rtable, _ in fks:
        if ptable != rtable:
            g.add_edge(ptable, rtable)

    pos = nx.spring_layout(g, k=2.8, iterations=300, seed=42)

    fig, ax = plt.subplots(figsize=(34, 26))
    colors = [CONTEXT_COLORS[g.nodes[n]["context"]] for n in g.nodes]
    sizes = [600 + 220 * g.degree(n) for n in g.nodes]

    nx.draw_networkx_edges(
        g, pos, ax=ax, edge_color="#c8c8c8", arrows=True,
        arrowsize=9, width=0.7, connectionstyle="arc3,rad=0.08",
    )
    nx.draw_networkx_nodes(
        g, pos, ax=ax, node_color=colors, node_size=sizes,
        edgecolors="white", linewidths=1.2, alpha=0.95,
    )
    nx.draw_networkx_labels(g, pos, ax=ax, font_size=7.5, font_weight="bold")

    handles = [
        mpatches.Patch(color=c, label=ctx)
        for ctx, c in CONTEXT_COLORS.items()
        if any(t.context == ctx for t in tables.values())
    ]
    ax.legend(handles=handles, loc="upper left", fontsize=13, title="Bounded context")
    ax.set_title(
        f"AeroMes — entity relationships ({len(tables)} tables, {len(fks)} FKs)",
        fontsize=20,
    )
    ax.axis("off")
    fig.tight_layout()
    fig.savefig(path, dpi=130)
    plt.close(fig)


def table_label(t: Table, max_cols: int = 14) -> str:
    """Table box text: name + PK/FK columns (the relationship-relevant ones)."""
    keys = [c for c in t.columns if c[0] in t.pk_columns or c[0] in t.fk_columns]
    lines = [t.name, "─" * max(len(t.name), 8)]
    for cname, ctype, _ in keys[:max_cols]:
        marks = ("PK " if cname in t.pk_columns else "") + (
            "FK " if cname in t.fk_columns else ""
        )
        lines.append(f"{marks or '   '}{cname}: {ctype}")
    rest = len(t.columns) - len(keys[:max_cols])
    if rest > 0:
        lines.append(f"… +{rest} more columns")
    return "\n".join(lines)


def draw_context(
    ctx: str, tables: dict[str, Table], fks: list, path: str
) -> None:
    members = {n for n, t in tables.items() if t.context == ctx}
    if not members:
        return
    # Include directly-referenced foreign tables as ghost nodes.
    ghosts: set[str] = set()
    edges: list[tuple[str, str, str]] = []
    for ptable, pcol, rtable, _ in fks:
        if ptable in members or rtable in members:
            edges.append((ptable, rtable, pcol))
            for n in (ptable, rtable):
                if n not in members:
                    ghosts.add(n)

    g = nx.DiGraph()
    g.add_nodes_from(members | ghosts)
    for p, r, _ in edges:
        if p != r:
            g.add_edge(p, r)

    import math

    n_nodes = len(g.nodes)
    side = max(12, int(n_nodes**0.5) * 7)
    # Rings: context tables inside (staggered over two radii so wide boxes
    # near the circle's top/bottom don't collide), referenced foreign tables
    # outside. Spring layout stacks large boxes; rings never overlap.
    inner = sorted(members)
    outer = sorted(ghosts)
    pos: dict[str, tuple[float, float]] = {}
    if len(inner) == 1:
        pos[inner[0]] = (0.0, 0.0)
    else:
        stagger = len(inner) > 12
        for i, n in enumerate(inner):
            a = 2 * math.pi * i / len(inner)
            r = 1.0 if (not stagger or i % 2 == 0) else 1.45
            pos[n] = (r * math.cos(a), r * math.sin(a))
    for i, n in enumerate(outer):
        a = 2 * math.pi * (i + 0.5) / max(len(outer), 1)
        pos[n] = (2.3 * math.cos(a), 2.3 * math.sin(a))

    fig, ax = plt.subplots(figsize=(side, side * 0.78))
    nx.draw_networkx_edges(
        g, pos, ax=ax, edge_color="#9a9a9a", arrows=True,
        arrowsize=12, width=0.9, connectionstyle="arc3,rad=0.1",
        min_source_margin=30, min_target_margin=30,
    )
    if len(edges) <= 30:  # labels become unreadable clutter on dense graphs
        edge_labels = {(p, r): c for p, r, c in edges if p != r}
        nx.draw_networkx_edge_labels(
            g, pos, ax=ax, edge_labels=edge_labels, font_size=6,
            font_color="#666666", bbox=dict(alpha=0),
        )

    for n in g.nodes:
        is_ghost = n in ghosts
        t = tables[n]
        text = n + f"\n({tables[n].context})" if is_ghost else table_label(t)
        ax.text(
            *pos[n], text,
            ha="center", va="center",
            fontsize=6.5 if not is_ghost else 7,
            family="monospace",
            fontweight="normal",
            bbox=dict(
                boxstyle="round,pad=0.45",
                facecolor="white" if not is_ghost else "#f0f0f0",
                edgecolor=CONTEXT_COLORS[t.context],
                linewidth=1.8 if not is_ghost else 1.0,
                linestyle="solid" if not is_ghost else "dashed",
            ),
        )

    ax.set_title(f"AeroMes — {ctx} context ({len(members)} tables)", fontsize=16)
    ax.axis("off")
    ax.margins(0.12)
    fig.tight_layout()
    fig.savefig(path, dpi=150)
    plt.close(fig)


def main() -> None:
    os.makedirs(OUT_DIR, exist_ok=True)
    tables, fks = fetch_schema()
    print(f"Schema loaded: {len(tables)} tables, {len(fks)} FK columns")

    by_ctx: dict[str, int] = defaultdict(int)
    for t in tables.values():
        by_ctx[t.context] += 1
    for ctx, n in sorted(by_ctx.items()):
        print(f"  {ctx:<12} {n} tables")

    overview = os.path.join(OUT_DIR, "overview.png")
    draw_overview(tables, fks, overview)
    print(f"wrote {os.path.relpath(overview)}")

    for ctx in CONTEXT_COLORS:
        if ctx == "Other":
            continue
        path = os.path.join(OUT_DIR, f"context-{ctx.lower()}.png")
        draw_context(ctx, tables, fks, path)
        if os.path.exists(path):
            print(f"wrote {os.path.relpath(path)}")


if __name__ == "__main__":
    main()
