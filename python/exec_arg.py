import sys
import json
import os
import re

config_dir = "\\ShooterGame\\Saved\\Config\\WindowsServer\\"

def main():
    arg = sys.argv

    # detail:サーバー設定を変える的な
    # arg1:edit（固定）
    # arg2:サーバーID
    # arg3:1又は2（GameUserSettings.iniとGame.iniを分ける）
    # arg4:変更するkey
    # arg5:変更するvalue
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
            print(datalist)
        if arg[3] == "1" and datalist == [] or arg[3] == "1" and datalist[0] != "[ServerSettings]":
            datalist.insert(0, "[ServerSettings]")
        if arg[3] == "2" and datalist == [] or arg[3] == "2" and datalist[0] != "[/script/shootergame.shootergamemode]":
            datalist.insert(0, "[/script/shootergame.shootergamemode]")
        change_flag = 0
        for i in range(len(datalist)):
            if re.match(arg[4],datalist[i]) == None:
                continue
            else:
                datalist[i][len(datalist[i])+1:] = arg[5]
                change_flag = 1
                break
        if change_flag == 0:
            datalist.insert(1, f"{arg[4]}={arg[5]}")
        with open(f"{install_dir}{config_dir}{setting_file}", mode="w") as f:
            f.write('\n'.join(datalist))
        return

# いつもの
if __name__ == "__main__":
    main()