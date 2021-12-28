import sys
import json
import os
import re

config_dir = "\\ShooterGame\\Saved\\Config\\WindowsServer\\"

def main():
    arg = sys.argv

    # detail：サーバー設定を変える的な（ファイルがなかった場合は作ります・設定ファイルが異常(空)だった場合も色々勝手に変更します・keyが存在しなかった場合は作ります）
    # arg1：edit（固定）
    # arg2：サーバーID
    # arg3：1又は2（GameUserSettings.iniとGame.iniを分ける）
    # arg4：変更するkey
    # arg5：変更するvalue
    # example：exec_arg.py edit 1 1 MaxPlayers 10
    if arg[1] == "edit":
        with open("settings.json", mode="r", encoding="utf-8") as f:
            settings = json.load(f)
        install_dir = settings[arg[2]]["dir"]
        if arg[3] == "1":
            setting_file = "GameUserSettings.ini"
        elif arg[3] == "2":
            setting_file = "Game.ini"
        else:
            print("unknown_arg3")
            return
        if os.path.isfile(f"{install_dir}{config_dir}{setting_file}") == False:
            with open(f"{install_dir}{config_dir}{setting_file}", mode="w") as f:
                pass
        with open(f"{install_dir}{config_dir}{setting_file}", mode="r", encoding="utf-8") as f:
            datalist = f.readlines()
        if arg[3] == "1" and datalist == [] or arg[3] == "1" and datalist[0] != "[ServerSettings]\n":
            datalist.insert(0, "[ServerSettings]\n")
        if arg[3] == "2" and datalist == [] or arg[3] == "2" and datalist[0] != "[/script/shootergame.shootergamemode]\n":
            datalist.insert(0, "[/script/shootergame.shootergamemode]\n")
        change_flag = 0
        for i in range(len(datalist)):
            if re.match(f"{arg[4]}=",datalist[i]) == None:
                continue
            else:
                datalist[i] = f"{datalist[i][:len(arg[4])]}={arg[5]}\n"
                change_flag = 1
                break
        if change_flag == 0:
            datalist.insert(1, f"{arg[4]}={arg[5]}\n")
        with open(f"{install_dir}{config_dir}{setting_file}", mode="w") as f:
            f.writelines(datalist)
        return

# いつもの
if __name__ == "__main__":
    main()