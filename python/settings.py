import sys
import json

#初回書き込み用のダミーデータ
first_setting = dict()
first_setting["value"] = 1
first_setting["1"] = {"name":"Server1","map":"Ragnarok","dir":"servers/s1"}

def main():
    arg = sys.argv

    ## CODE：python -m settings.py first
    ## EXEC：設定ファイルを生成
    if arg[1] == "first":
        with open("settings.json", mode='w', encoding="utf-8") as f:
            json.dump(first_setting, f, ensure_ascii=False, indent=4)
        # print(f'{first_setting["1"]["name"]},{first_setting["1"]["map"]},{first_setting["1"]["dir"]}')
        return
    
    if arg[1] == "value":
        with open("settings.json", mode="r", encoding="utf-8") as f:
            settings = json.load(f)
        print(settings["value"])
        return

    ## CODE：python -m settings.py read [NUM]
    ## EXEC：設定ファイルからNUM番目のサーバーの情報を返す
    if arg[1] == "read":
        with open("settings.json", mode="r", encoding="utf-8") as f:
            settings = json.load(f)
        if int(settings["value"]) < int(arg[2]):
            print("over")
            return
        print(f'{settings[arg[2]]["name"]},{settings[arg[2]]["map"]},{settings[arg[2]]["dir"]}')
        return

    ## CODE：python -m settings.py write [NAME] [MAP] [DIR]
    ## EXEC：設定ファイルに指定した設定データを追記
    if arg[1] == "write":
        with open("settings.json", mode="r", encoding="utf-8") as f:
            settings = json.load(f)
        settings["value"] = settings["value"] + 1
        settings[f"{settings['value']}"] = {
                "name":arg[2],
                "map":arg[3],
                "dir":arg[4]
            }
        with open("settings.json", mode='w', encoding="utf-8") as f:
            json.dump(settings, f, ensure_ascii=False, indent=4)
    
    ## CODE：python -m settings.py edit [NUM] [NAME] [MAP] [DIR]
    ## EXEC：設定ファイルの指定したNUMのデータを上書き
    if arg[1] == "edit":
        with open("settings.json", mode="r", encoding="utf-8") as f:
            settings = json.load(f)
        settings[str(arg[2])] = {
                "name":arg[3],
                "map":arg[4],
                "dir":arg[5]
            }
        with open("settings.json", mode='w', encoding="utf-8") as f:
            json.dump(settings, f, ensure_ascii=False, indent=4)


# プログラム実行時のみ実行する
if __name__ == "__main__":
    main()