import psutil
import re
import os

def main():
    for p in psutil.process_iter(attrs=('name', 'pid', 'cmdline')):
        if p.info["pid"] == os.getpid():
            p.terminate()
    return

if __name__ == '__main__':
    main()