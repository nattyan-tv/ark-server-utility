#coding:utf-8
from multiprocessing.connection import Client
import socket, threading

"""IPC通信（送信用）コンソール
"""

print("IPC Connector (Client)")

def main(port):
    while True:
        command = input(f"localhost:{port}> ")
        try:
            data = str(command).encode()
            length = len(data)
            client.sendall(length.to_bytes(4, byteorder='big'))
            client.sendall(data)
            data = client.recv(4)
            length = int.from_bytes(data, "big")
            data = client.recv(length)
            msg = data.decode()
            print(msg)
        except BaseException as err:
            print(f"error happend.\n{err}")

if __name__ == "__main__":
    print("address> localhost")
    port = input("port> ")
    if port == "":
        print("please insert a port.")
        input("Enter to exit...")
        exit()
    client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    try:
        client.connect(("localhost", int(port)))
        print(f"connect [localhost:{port}]!")
    except BaseException as err:
        print(f"error happend.\n{err}")
        input("Enter to exit...")
        exit()
    main(port)