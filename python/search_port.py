import psutil, os, sys

start = 49152
def main():
    if sys.argv[1] == "1":
        used_ports = [conn.laddr.port for conn in psutil.net_connections() if conn.status == 'LISTEN']
        for port in range(start, 65535 + 1):
            if port not in set(used_ports):
                return port

    elif sys.argv[1] == "2":
        ports = []
        cont = 0
        cont_port = 0
        used_ports = [conn.laddr.port for conn in psutil.net_connections() if conn.status == 'LISTEN']
        for port in range(1, 65535 + 1):
            if port not in set(used_ports):
                if len(ports) != 0 and ports[-1]+1 == port and cont != 1:
                    ports[-1] = port-2
                    cont_port = port
                    cont = 1
                    continue
                elif cont == 1 and cont_port+1 == port:
                    cont_port = port
                    continue
                elif cont == 1 and cont_port+1 != port:

                    ports.append("-")
                    ports.append(cont_port)
                    cont_port = 0
                    cont = 0
                    
                    continue
                ports.append(port)
        
        if cont == 1:
            ports.append("-")
            ports.append(cont_port)
        ports = str(ports)[1:-1].replace(", '", "").replace("', ", "")
        return ports

if __name__ == '__main__':
    for p in psutil.process_iter(attrs=('name', 'pid', 'cmdline')):
        if p.info["name"] == "ipc_main.exe" and p.info["pid"] != os.getpid():
            p.terminate()
    print(main())