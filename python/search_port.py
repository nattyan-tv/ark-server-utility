import psutil, os

start = 49152
def main():
    used_ports = [conn.laddr.port for conn in psutil.net_connections() if conn.status == 'LISTEN']
    for port in range(start, 65535 + 1):
        if port not in set(used_ports):
            return port

if __name__ == '__main__':
    for p in psutil.process_iter(attrs=('name', 'pid', 'cmdline')):
        if p.info["name"] == "ipc_main.exe" and p.info["pid"] != os.getpid():
            p.terminate()
    print(main())