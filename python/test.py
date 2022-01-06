import subprocess, os, shutil

files = len(os.listdir("python"))
for i in range(files):
    if files[i][-3:] != ".py":
        continue
    subprocess.run(["pyinstaller", f"python/{files[i]}"])

shutil.copytree("python/dist", "bin/Debug/python")