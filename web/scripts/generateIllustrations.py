#!/usr/bin/env python3
# /// script
# requires-python = ">=3.11"
# dependencies = [
#   "openai>=1.50.0",
#   "httpx>=0.27.0",
# ]
# ///
"""
Generate AeroMes illustration assets from prompt files using DALL-E 3.

Usage:
  OPENAI_API_KEY=sk-... uv run scripts/generateIllustrations.py
  OPENAI_API_KEY=sk-... uv run scripts/generateIllustrations.py --only auth-hero error-404

Each prompt file in src/assets/illustrations/prompts/ declares the target filename,
dimensions, and a DALL-E generation prompt. Generated PNGs are saved alongside the
prompt files in src/assets/illustrations/.
"""

import argparse
import os
import re
import sys
from pathlib import Path

import httpx
from openai import OpenAI

SCRIPT_DIR = Path(__file__).parent
ILLUSTRATIONS_DIR = SCRIPT_DIR.parent / "src" / "assets" / "illustrations"
PROMPTS_DIR = ILLUSTRATIONS_DIR / "prompts"

# DALL-E 3 supports only these sizes; we pick the closest fit per illustration.
DALLE_SIZES = {
    "square": "1024x1024",
    "landscape": "1792x1024",
    "portrait": "1024x1792",
}


def pick_size(width: int, height: int) -> str:
    ratio = width / height
    if ratio > 1.3:
        return DALLE_SIZES["landscape"]
    if ratio < 0.77:
        return DALLE_SIZES["portrait"]
    return DALLE_SIZES["square"]


def parse_prompt_file(path: Path) -> dict:
    text = path.read_text(encoding="utf-8")

    filename = re.search(r"FILE:\s*(\S+)", text)
    dimensions = re.search(r"DIMENSIONS:\s*(\d+)\s*x\s*(\d+)", text)
    prompt_match = re.search(r"PROMPT:\s*\n(.+)", text, re.DOTALL)

    if not (filename and dimensions and prompt_match):
        raise ValueError(f"Malformed prompt file: {path}")

    w, h = int(dimensions.group(1)), int(dimensions.group(2))
    return {
        "filename": filename.group(1),
        "width": w,
        "height": h,
        "dalle_size": pick_size(w, h),
        "prompt": prompt_match.group(1).strip(),
    }


def generate_and_save(client: OpenAI, spec: dict, output_dir: Path, force: bool) -> bool:
    out_path = output_dir / spec["filename"]
    if out_path.exists() and not force:
        print(f"  skip  {spec['filename']} (already exists; use --force to overwrite)")
        return False

    print(f"  gen   {spec['filename']}  [{spec['dalle_size']}]  …", flush=True)
    response = client.images.generate(
        model="dall-e-3",
        prompt=spec["prompt"],
        n=1,
        size=spec["dalle_size"],
        response_format="url",
        quality="standard",
    )

    url = response.data[0].url
    image_bytes = httpx.get(url, timeout=60).content
    out_path.write_bytes(image_bytes)
    print(f"  saved {out_path.relative_to(ILLUSTRATIONS_DIR.parent.parent.parent)}")
    return True


def main():
    parser = argparse.ArgumentParser(description="Generate AeroMes illustrations via DALL-E 3")
    parser.add_argument(
        "--only",
        nargs="+",
        metavar="STEM",
        help="Generate only the named illustrations (stem without .png), e.g. --only auth-hero error-404",
    )
    parser.add_argument(
        "--force",
        action="store_true",
        help="Overwrite existing PNG files",
    )
    args = parser.parse_args()

    api_key = os.environ.get("OPENAI_API_KEY")
    if not api_key:
        print("ERROR: OPENAI_API_KEY environment variable is not set.", file=sys.stderr)
        print("  Set it: export OPENAI_API_KEY=sk-...", file=sys.stderr)
        sys.exit(1)

    client = OpenAI(api_key=api_key)

    prompt_files = sorted(PROMPTS_DIR.glob("*.txt"))
    if not prompt_files:
        print(f"No .txt files found in {PROMPTS_DIR}", file=sys.stderr)
        sys.exit(1)

    # Filter if --only specified
    if args.only:
        wanted = {s.lower().removesuffix(".png") for s in args.only}
        prompt_files = [p for p in prompt_files if p.stem in wanted]
        if not prompt_files:
            print(f"No prompt files matched: {args.only}", file=sys.stderr)
            sys.exit(1)

    print(f"Generating {len(prompt_files)} illustration(s) → {ILLUSTRATIONS_DIR.relative_to(Path.cwd())}")
    generated = 0
    errors = []

    for pf in prompt_files:
        try:
            spec = parse_prompt_file(pf)
            ok = generate_and_save(client, spec, ILLUSTRATIONS_DIR, args.force)
            if ok:
                generated += 1
        except Exception as exc:
            print(f"  ERROR {pf.name}: {exc}", file=sys.stderr)
            errors.append(pf.name)

    print(f"\nDone: {generated} generated, {len(prompt_files) - generated - len(errors)} skipped, {len(errors)} errors.")
    if errors:
        print("Failed:", ", ".join(errors), file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
