import psutil
import re

def main():
    for p in psutil.process_iter(attrs=('name', 'pid', 'cmdline')):
        if re.search("python",p.info['name']) == None:
            continue
        print(p.info["cmdline"][1])
        if re.search("ipc_main.py", p.info["cmdline"][1]):
            p.terminate()
    return

if __name__ == '__main__':
    main()