import sys
import json
import requests

main_url = "http://arkdedicated.com/"

def main():
    arg = sys.argv

    if arg[1] == "version" and arg[2] == "0":
        url = main_url + "version"
        req = requests.get(url)
        version = req.json()
        print(version)

    if arg[1] == "version" and arg[2] == "1":
        url = main_url + "version"
        req = requests.get(url)
        latest_version = req.json()
        with open("settings.json", mode="r", encoding="utf-8") as f:
            settings = json.load(f)
        locate = settings["1"]["dir"]
        with open(locate + "\\version.txt", mode="r", encoding="utf-8") as f:
            current_version = f.read()
        print(f"{latest_version},{current_version}")


# プログラム実行時のみ実行する
if __name__ == "__main__":
    main()
