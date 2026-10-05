import json, os, sys
jp = sys.argv[1] if len(sys.argv) > 1 else "Assets/StreamingAssets/campus_data.json"
imgdir = sys.argv[2] if len(sys.argv) > 2 else "Assets/Images/Hotspot Popup Images"
d = json.load(open(jp, encoding="utf-8"))
errs, warns = [], []
imgs = {os.path.splitext(f)[0].lower() for f in os.listdir(imgdir)} if os.path.isdir(imgdir) else None
if imgs is None: warns.append(f"image folder '{imgdir}' not found - image names not checked")
ids = [l.get("id") for l in d.get("locations", [])]
if len(ids) != len(set(ids)): errs.append("duplicate location ids")
for l in d["locations"]:
    i = l.get("id", "?")
    for k in ("id", "scene_name", "label", "hotspots", "links"):
        if k not in l: errs.append(f"{i}: missing '{k}'")
    hs = l.get("hotspots", [])
    if len(hs) > 8: errs.append(f"{i}: {len(hs)} hotspots (max 8)")
    hids = [h.get("id") for h in hs]
    if len(hids) != len(set(hids)): errs.append(f"{i}: duplicate hotspot ids")
    for h in hs:
        n = f"{i}/{h.get('id')}"
        if not h.get("id"): errs.append(f"{i}: hotspot without id")
        if not (h.get("title") or h.get("content")): errs.append(f"{n}: needs title or content")
        wp = h.get("world_position")
        if not wp or not all(a in wp for a in "xyz"): errs.append(f"{n}: world_position needs x,y,z")
        elif (wp["x"]**2 + wp["z"]**2) ** .5 > 4.5: warns.append(f"{n}: more than 4.5 m from centre (will be pulled in)")
        if imgs is not None and h.get("image") and h["image"].lower() not in imgs: errs.append(f"{n}: image '{h['image']}' not in popup folder")
        t = h.get("target_id")
        if t and t not in ids: errs.append(f"{n}: target_id '{t}' unknown")
    for lk in l.get("links", []):
        if lk.get("target_id") not in ids: errs.append(f"{i}: link to unknown '{lk.get('target_id')}'")
        if not isinstance(lk.get("yaw"), (int, float)): errs.append(f"{i}: link yaw must be a number")
for w in warns: print("WARN ", w)
for e in errs: print("ERROR", e)
print(f"{'FAILED' if errs else 'OK'} - {len(ids)} locations, {len(errs)} errors, {len(warns)} warnings")
sys.exit(1 if errs else 0)
 
