import re

with open("D:/vs_repos/ark_server_utility/python/data.txt", mode="r", encoding="utf-8") as f:
    data = f.read()
data = data.split("\n")
main = str()
for i in range(len(data)-1):
    data_c = data[i].split(", ")
    # ?引数=デフォルト, 特になし, 説明, Game.ini, 特になし
    # -引数, 特になし, 説明, Game.ini, 特になし
    # 引数, タイプ, デフォルト, 説明, GameUserSettings.ini
    print(data_c)
    if data_c[3] == "Game.ini":
        if data_c[0][:1] != "-":
            arg_type = 1
            print(data_c[0][1:])
            if re.search("=", data_c[0][1:]):
                data_ca = data_c[0][1:].split("=")[0]
                example = data_c[0][1:].split("=")[1]
            else:
                data_ca = data_c[0][1:]
                example = "値はない"
            data_c[2] = data_c[2]
            file_name = "Game.ini"
        else:
            continue
    else:
        continue
    #     elif data_c[0][:1] == "-":
    #         arg_type = 2
    #         example = ""
    #         data_ca = data_c[0][1:]
    #         data_c[2] = data_c[2]
    #         file_name = "Game.ini"
    # elif data_c[4] == "GameUserSettings.ini":
    #     data_ca = data_c[0]
    #     data_c[2] = data_c[2]
    #     example = data_c[3]
    #     file_name = data_c[4]
    #     arg_type = data_c[1]
    #     ####################
    #     data_c[2] = example
    # file_name = "GameUserSettings.ini"
    # main = main + "\n" + f'dict.Add("{data_ca}", "{data_c[2]}", "{example}", "{file_name}", {arg_type});'
    # arg_setting.Items.Add(new arg_data { arg = "aaaa", detail = "bbb"});
    main = main + "\n" + 'arg_setting.Items.Add(new arg_data { arg = "' + data_ca + '", detail = "' + data_c[2] + '"});'

with open("exports.txt", mode="w", encoding="utf-8") as f:
    f.write(main)
# 今回やったのは、「?」系の起動オプション

