import socket, threading, sys, os
import psutil, json, requests, re

#初回設定書き込み用のダミーデータ
first_setting = dict()
first_setting["value"] = 1
first_setting["1"] = {"name":"server1","map":"ThsIsland","dir":"C:\\"}

# 設定ファイルの場所
config_dir = "\\ShooterGame\\Saved\\Config\\WindowsServer\\"

def binder(client_socket, addr):
    try:
        while True:
            data = client_socket.recv(4)
            length = int.from_bytes(data, "big")
            data = client_socket.recv(length)
            msg = data.decode()
            print([msg])
            rt_msg = msg


            # IPC通信・自プログラムを終了する
            if msg == "exit":
                server_socket.close()
                for p in psutil.process_iter(attrs=('name', 'pid', 'cmdline')):
                    if p.info["pid"] == os.getpid():
                        p.terminate()


            if msg[0:8] == "settings":

                # 初回起動時のダミーデータを書き込む
                # settings first
                # OK
                if msg[9:14] == "first":
                    with open("settings.json", mode='w', encoding="utf-8") as f:
                        json.dump(first_setting, f, ensure_ascii=False, indent=4)
                    rt_msg = "OK"

                # サーバー数を呼び出す
                # settings value
                # [サーバー数]
                elif msg[9:14] == "value":
                    with open("settings.json", mode="r", encoding="utf-8") as f:
                        settings = json.load(f)
                    rt_msg = settings["value"]

                # サーバーの名前一覧を呼び出す
                # settings name
                # [名前],[名前],[名前]...
                elif msg[9:13] == "name":
                    se = ""
                    with open("settings.json", mode="r", encoding="utf-8") as f:
                        settings = json.load(f)
                    for i in range(settings["value"]):
                        se = se + "," + settings[f"{i+1}"]["name"]
                    se = se[1:]
                    rt_msg = se

                # サーバーの設定を呼び出す
                # settings read [NUM]
                # [サーバー名],[サーバーマップ],[サーバーディレクトリ]
                elif msg[9:13] == "read":
                    arg = msg[14:].split(" ")
                    with open("settings.json", mode="r", encoding="utf-8") as f:
                        settings = json.load(f)
                    if int(settings["value"]) < int(arg[0]):
                        rt_msg = "over"
                    rt_msg = f'{settings[arg[0]]["name"]},{settings[arg[0]]["map"]},{settings[arg[0]]["dir"]}'
                
                # サーバーの設定を追記する
                # settings write [NAME] [MAP] [DIR]
                # OK
                elif msg[9:14] == "write":
                    arg = msg[15:].split(" ")
                    with open("settings.json", mode="r", encoding="utf-8") as f:
                        settings = json.load(f)
                    settings["value"] = settings["value"] + 1
                    settings[f"{settings['value']}"] = {
                            "name":arg[0],
                            "map":arg[1],
                            "dir":arg[2]
                        }
                    with open("settings.json", mode='w', encoding="utf-8") as f:
                        json.dump(settings, f, ensure_ascii=False, indent=4)
                    rt_msg = "OK"

                # サーバーの設定を編集する
                # settings edit [NUM] [NAME] [MAP] [DIR]
                # OK
                elif msg[9:13] == "edit":
                    arg = msg[14:].split(" ")
                    with open("settings.json", mode="r", encoding="utf-8") as f:
                        settings = json.load(f)
                    settings[str(arg[0])] = {
                            "name":arg[1],
                            "map":arg[2],
                            "dir":arg[3]
                        }
                    with open("settings.json", mode='w', encoding="utf-8") as f:
                        json.dump(settings, f, ensure_ascii=False, indent=4)
                    rt_msg = "OK"
                
                # サーバーの設定を削除する
                # settings del [NUM]
                # OK
                elif msg[9:12] == "del":
                    num = msg[13:]
                    try:
                        with open("settings.json", mode="r", encoding="utf-8") as f:
                            settings = json.load(f)
                        settings["value"] = settings["value"] - 1
                        for i in range(int(settings["value"]) - int(num)):
                            settings[f"{int(num) + i}"] = settings[f"{int(num) + i + 1}"]
                        del settings[f"{settings['value']+1}"]
                        print(settings)
                        with open("settings.json", mode='w', encoding="utf-8") as f:
                            json.dump(settings, f, ensure_ascii=False, indent=4)
                        rt_msg = "OK"
                    except BaseException as err:
                        print(err)



            elif msg[0:6] == "webapi":
                arg = msg[7:].split(" ")
                # 最新バージョンを取得
                # webapi version 0
                # [配信されているバージョン]
                if arg[0] == "version" and arg[1] == "0":
                    url = "http://arkdedicated.com/version"
                    req = requests.get(url)
                    version = req.json()
                    rt_msg = version

                # 最新バージョンと現行バージョンを取得
                # webapi version [NUM]
                # [配信されているバージョン],[インストールされているバージョン]
                elif arg[0] == "version" and arg[1] != "0":
                    url = "http://arkdedicated.com/version"
                    req = requests.get(url)
                    latest_version = req.json()
                    with open("settings.json", mode="r", encoding="utf-8") as f:
                        settings = json.load(f)
                    locate = settings[str(arg[1])]["dir"]
                    with open(locate + "\\version.txt", mode="r", encoding="utf-8") as f:
                        current_version = f.read()
                    rt_msg = f"{latest_version},{current_version}"
                
                elif arg[0] == "system":
                    try:
                        url = requests.get("https://nattyan-tv.github.io/ark-server-utility/info.json")
                        text = url.text
                        data = json.loads(text)
                        rt_msg = data["version"]
                    except BaseException as err:
                        rt_msg = f"ERR:{err}"


            elif msg[0:8] == "exec_arg":
                arg = msg[9:].split(" ")
                # サーバー設定を変える的な（ファイルがなかった場合は作ります・設定ファイルが異常(空)だった場合も色々勝手に変更します・keyが存在しなかった場合は作ります）
                # edit [NUM] [FILE_TYPE] [KEY] [VALUE]
                ## ※FILE_TYPEは1又は2（GameUserSettings.iniとGame.iniを分ける）
                # OK
                if arg[0] == "edit":
                    with open("settings.json", mode="r", encoding="utf-8") as f:
                        settings = json.load(f)
                    install_dir = settings[arg[1]]["dir"]
                    if arg[2] == "1":
                        setting_file = "GameUserSettings.ini"
                    elif arg[2] == "2":
                        setting_file = "Game.ini"
                    else:
                        rt_msg = "unknown_arg(3)"
                    if os.path.isfile(f"{install_dir}{config_dir}{setting_file}") == False:
                        with open(f"{install_dir}{config_dir}{setting_file}", mode="w") as f:
                            pass
                    with open(f"{install_dir}{config_dir}{setting_file}", mode="r", encoding="utf-8") as f:
                        datalist = f.readlines()
                    if arg[2] == "1" and datalist == [] or arg[2] == "1" and datalist[0] != "[ServerSettings]\n":
                        datalist.insert(0, "[ServerSettings]\n")
                    if arg[2] == "2" and datalist == [] or arg[2] == "2" and datalist[0] != "[/script/shootergame.shootergamemode]\n":
                        datalist.insert(0, "[/script/shootergame.shootergamemode]\n")
                    change_flag = 0
                    for i in range(len(datalist)):
                        if re.match(f"{arg[3]}=",datalist[i]) == None:
                            continue
                        else:
                            datalist[i] = f"{datalist[i][:len(arg[3])]}={arg[4]}\n"
                            change_flag = 1
                            break
                    if change_flag == 0:
                        datalist.insert(1, f"{arg[3]}={arg[4]}\n")
                    with open(f"{install_dir}{config_dir}{setting_file}", mode="w") as f:
                        f.writelines(datalist)
                    rt_msg = "OK"
            

            elif msg[0:5] == "debug":
                arg = msg[6:].split(" ")
                if arg [0] == "pid":
                    rt_msg = os.getpid()
                elif arg[0] == "addr":
                    rt_msg = [addr,sys.argv[1]]
            
            if msg != "":
                # rt_msg = f"[{msg}]"
                pass
            

            print([rt_msg])
            data = str(rt_msg).encode()
            length = len(data)
            client_socket.sendall(length.to_bytes(4, byteorder='big'))
            client_socket.sendall(data)
    except:
        pass
    finally:
        client_socket.close()



if __name__ == "__main__":
    if len(sys.argv) == 1:
        print("Port:None")
        sys.exit(0)
    for p in psutil.process_iter(attrs=('name', 'pid', 'cmdline')):
        if p.info["name"] == "ipc_main.exe" and p.info["pid"] != os.getpid():
            p.terminate()
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

    server_socket.bind(('', int(sys.argv[1])))
    print("Port:" + sys.argv[1])
    server_socket.listen()

    try:
        while True:
            client_socket, addr = server_socket.accept()
            th = threading.Thread(target=binder, args = (client_socket,addr))
            th.start()
    except:
        pass
    finally:
        server_socket.close()
