import sys
import requests
import zipfile
import shutil
import os
from tkinter import messagebox

url='https://github.com/nattyan-tv/ark-server-utility/releases/latest/download/ark-server-utility.zip'

filename='ark-server-utility.zip'
path = "./update"

def main():
    print(sys.argv)

    if len(sys.argv) >= 2 and sys.argv[1] == "true":
        try:
            urlData = requests.get(url).content
            with open(filename ,mode='wb') as f:
                f.write(urlData)
            if not os.path.exists(path):
                with zipfile.ZipFile(filename) as zip:
                    zip.extractall(path)
            else:
                shutil.rmtree(path)
                with zipfile.ZipFile(filename) as zip:
                    zip.extractall(path)
            for i in range(len(os.listdir(path))):
                print([os.listdir(path)[i],os.path.isfile(f"{path}/{os.listdir(path)[i]}")])
                if os.path.isfile(f"{path}/{os.listdir(path)[i]}"):
                    shutil.copy2(f"{path}/{os.listdir(path)[i]}", "./")
                elif os.path.isdir(f"{path}/{os.listdir(path)[i]}"):
                    shutil.rmtree(f"./{os.listdir(path)[i]}")
                    shutil.copytree(f"{path}/{os.listdir(path)[i]}", f"./{os.listdir(path)[i]}")
            os.remove(f"./{filename}")
            messagebox.showinfo("アップデート成功", "アップデートに成功しました。")
            return
        except BaseException as err:
            messagebox.showerror("アップデート失敗", f"アップデート操作中にエラーが発生しました。\n{err}")
            rt = messagebox.askretrycancel("アップデート失敗", "再試行しますか？")
            if rt == True:
                main()
                return
            else:
                return
    
    else:
        messagebox.showerror("アップデーター", "アップデートはARK: Server Utilityから行えます。")
        return

if __name__ == "__main__":
    main()