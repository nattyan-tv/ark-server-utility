import sys
import json

#初回起動時に、設定ファイルを書き込む
first_setting = dict()
first_setting[1] = {"name":"Server1","map":"Ragnarok","dir":"servers/s1"}

if sys.argv[1] == "first":
    with open("settings.json", mode='w', encoding="utf-8") as f:
        json.dump(first_setting, f, ensure_ascii=False, indent=4)
    print(first_setting)

