#coding:utf-8
import discord
import sys, os
import json

intents = discord.Intents.default()
intents.typing = False
bot = commands.Bot(command_prefix=PREFIX, intents=intents, help_command=None)

if os.path.isfile("bot.setting.json") == False:
    exit()

SETTING = json.load(open("bot.setting.json", "r"))

TOKEN = SETTING["TOKEN"]
PREFIX = SETTING["PREFIX"]

if SETTING["GUILD"] != "None":
    GUILD = SETTING["GUILD"].split(",")
else:
    GUILD = None

if SETTING["USER"] != "None":
    USER = SETTING["USER"].split(",")
else:
    USER = None

BOT = SETTING["BOT"]

def cmd_bool(ctx: discord.command.Context) -> bool:
    guild = ctx.guild.id
    user = ctx.author.id
    bot = ctx.author.bot
    if BOT == False and bot == True:
        return False
    if GUILD == None and USER == None:
        return True
    if GUILD != None and USER == None:
        if guild in GUILD:
            return True
        else:
            return False
    if GUILD == None and USER != None:
        if user in USER:
            return True
        else:
            return False
    if GUILD != None and USER != None:
        if guild in GUILD and user in USER:
            return True
        else:
            return False

@bot.event
async def on_ready():
    print("bot_launched")

@bot.event
async def on_error(event):
    print(sys.exc_info())

@bot.command()
async def start(ctx: discord.command.Context):
    if cmd_bool(ctx):
        if ctx.message.content == f"{PREFIX}start":
            await ctx.reply("エラー", embed=discord.Embed(title="引数が足りません。", description=f"```{PREFIX}start [サーバーID]```", color=0xff0000))
            return
        server = ctx.message.content.split(" ",1)[1]
        try:
            server_id = int(server)
        except BaseException as err:
            await ctx.reply("エラー", embed=discord.Embed(title="サーバーIDは数値でなければなりません。", description=f"```{err}```", color=0xff0000))
            return

@bot.command()
async def help(ctx: discord.command.Context):
    if cmd_bool(ctx):
        embed = discord.Embed(title="HELP",description="discord support bot",color=0x6666ff)
        embed.add_field(name="`コマンド一覧`", value="対応しているコマンド一覧", inline=False)
        embed.add_field(name=f"{PREFIX}start [ServerID]", value="指定したサーバーを起動します。", inline=False)


if __name__ == "__main__":
    bot.run(TOKEN)
