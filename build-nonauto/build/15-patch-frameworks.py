import os
import sys

projdir = os.environ["PROJDIR"]
xcodedir = os.environ["XCODEDIR"]

execfile(os.path.join(projdir, "build-nonauto", "util", "pbxproj.py"))

proj = Pbxproj(os.path.join(xcodedir, "Unity-iPhone.xcodeproj"))

for x in range(1, len(sys.argv)):
    if(proj.add_framework(sys.argv[x] + ".framework") is False):
        exit(1)
