import subprocess, os, shutil

# PYINSTALLERを実行するためだけのやつ

# make_list = ["arg_data.py", "exec_arg.py", "ipc_main.py", "kill_ipc.py", "search_port.py", "settings.py", "webapi.py"]
make_list = ["search_port.py"]
files = os.listdir()
print(files)
for i in range(len(files)):
    if files[i] not in make_list:
        continue
    print(files[i])
    subprocess.run(["pyinstaller", f"{files[i]}", "--onefile", "--noconsole"])
