#!/usr/bin/env python3
# /// script
# requires-python = ">=3.11"
# dependencies = [
#   "Pillow>=10.0.0",
# ]
# ///
"""
Create placeholder illustration PNGs so the Vite build passes before
real images are generated with generateIllustrations.py.

Each placeholder is a solid-teal rectangle at the correct dimensions,
with the filename printed in the center. Safe to overwrite — the real
generator (generateIllustrations.py) will replace these.

Usage:  uv run scripts/stubIllustrations.py
"""

from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

SCRIPT_DIR = Path(__file__).parent
ILLUSTRATIONS_DIR = SCRIPT_DIR.parent / "src" / "assets" / "illustrations"

BG_COLOR = (4, 74, 66)       # #044A42 — primary teal
TEXT_COLOR = (58, 145, 136)  # #3A9188 — lighter teal

STUBS = [
    ("auth-hero.png",         1200, 900),
    ("dashboard-ambient-bg.png", 1920, 1080),
    ("empty-no-data.png",     400, 300),
    ("empty-no-results.png",  400, 300),
    ("empty-no-orders.png",   400, 300),
    ("error-403.png",         400, 300),
    ("error-404.png",         400, 300),
    ("error-500.png",         400, 300),
    ("tablet-idle-welcome.png", 800, 400),
]


def make_stub(filename: str, width: int, height: int) -> None:
    out = ILLUSTRATIONS_DIR / filename
    if out.exists():
        print(f"  skip  {filename} (exists)")
        return

    img = Image.new("RGB", (width, height), BG_COLOR)
    draw = ImageDraw.Draw(img)

    label = filename
    try:
        font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", size=max(16, height // 20))
    except Exception:
        font = ImageFont.load_default()

    bbox = draw.textbbox((0, 0), label, font=font)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    draw.text(((width - tw) // 2, (height - th) // 2), label, fill=TEXT_COLOR, font=font)

    img.save(out, "PNG", optimize=True)
    print(f"  stub  {filename}  [{width}×{height}]")


def main():
    ILLUSTRATIONS_DIR.mkdir(parents=True, exist_ok=True)
    print(f"Creating placeholder illustrations → {ILLUSTRATIONS_DIR.relative_to(Path.cwd())}")
    for name, w, h in STUBS:
        make_stub(name, w, h)
    print("Done. Run 'npm run generate:illustrations' to replace with AI-generated images.")


if __name__ == "__main__":
    main()
