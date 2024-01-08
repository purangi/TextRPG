using System;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace TextRPG
{
    internal class Program
    {
        public interface ICharacter //캐릭터, 몬스터 등 공용
        {
            string Name { get; set; }
            int AtkPower { get; set; }
            int DefPower { get; set; }
            int Health { get; set; }

            void Attack(ICharacter Enemy);
        }

        public class Warrior : ICharacter
        {
            public int Level { get; set; }
            public string Name { get; set; }
            public int Job { get; set; }
            public int AtkPower { get; set; }
            public int DefPower { get; set; }
            public int Health { get; set; }
            public int Gold { get; set; }
            public Item AtkWeapon { get; set; } //무기
            public Item DefArmor { get; set; } //갑옷
            public int maxHP { get;}

            public List<Item> inven = new List<Item>();

            //전사, 용병, 마법사 스탯
            string[] jobs = { "전사", "용병", "마법사" };
            int[] atkPowers = { 10, 8, 12 };
            int[] defPowers = { 7, 10, 5 };
            int[] healthes = { 100, 80, 50 };
            int[] golds = { 1500, 1200, 2000 };

            public Warrior(string name, int job)
            {
                Level = 1;
                Name = name;
                Job = job;
                AtkPower = atkPowers[job - 1];
                DefPower = defPowers[job - 1];
                Health = healthes[job - 1];
                Gold = golds[job - 1];
                maxHP = Health;
            }

            public void Attack(ICharacter Enemy)
            {
                Random rand = new Random();
                int atk = AtkPower;
                if(AtkWeapon != null)
                {
                    atk += AtkWeapon.ItemAtk;
                }
                double damage = (atk * rand.Next(80, 121)) * 0.01;

                if (damage - Enemy.DefPower < damage / 2)
                {
                    damage /= 2;
                }
                else
                {
                    damage -= Enemy.DefPower;
                }

                Enemy.Health -= (int) Math.Round(damage);
                Console.WriteLine("{0} 은/는 {1}의 데미지를 받았다!", Enemy.Name, (int)Math.Round(damage));
            }

            public void LevelUP()
            {
                Level++;
                Console.WriteLine("Level " + Level + "로 레벨업 하였습니다.");
            }

            public void Heal()
            {
                Health = maxHP;
                Console.WriteLine("체력을 모두 회복했습니다.");
            }

            public void ShowStatus()
            {
                TextColor("\n상태 보기", ConsoleColor.Yellow);
                Console.WriteLine("캐릭터의 정보가 표시됩니다.\n");
                Console.WriteLine("    Lv. " + Level.ToString("D2"));
                Console.WriteLine("    " + Name + " ( " + jobs[Job - 1] + " )");
                Console.Write("    공격력 : " + AtkPower);
                if(AtkWeapon != null)
                {
                    TextColor(" (+" + AtkWeapon.ItemAtk + ")", ConsoleColor.Yellow);
                } else
                {
                    Console.WriteLine();
                }
                Console.Write("    방어력 : " + DefPower);
                if (DefArmor != null)
                {
                    TextColor(" (+" + DefArmor.ItemDef + ")", ConsoleColor.Yellow);
                }
                else
                {
                    Console.WriteLine();
                }
                Console.WriteLine("    체  력 : " + Health + " / " + maxHP);
                Console.WriteLine("    골  드 : {0} G", Gold);
            }

            public void EquipItem(int num)
            {
                Item item = inven[num - 1];

                //item을 벗기
                if(item.isEquipped)
                {
                    item.isEquipped = false;
                    //벗기
                    if (item.ItemPart == 0)
                    {
                        DefArmor = null;
                    }
                    else if (item.ItemPart == 1)
                    {
                        AtkWeapon = null;
                    }
                    Console.WriteLine("\n'{0}'을 장착 해제했습니다.", item.ItemName);
                } else //장착
                {
                    //입기
                    if (item.ItemPart == 0)
                    {
                        if(DefArmor != null)
                        {
                            DefArmor.isEquipped = false;
                        }
                        DefArmor = item;
                    }
                    else if (item.ItemPart == 1)
                    {
                        if (AtkWeapon != null)
                        {
                            AtkWeapon.isEquipped = false;
                        }
                        AtkWeapon = item;
                    }
                    item.isEquipped = true;
                    Console.WriteLine("\n'{0}'을 장착했습니다.", item.ItemName);
                }
            }

            public int ShowInven(bool isManaging)
            {
                Console.WriteLine("\n[아이템 목록]\n");

                if (inven.Count <= 0)
                {
                    Console.WriteLine("\n인벤토리에 아이템이 없습니다.\n상점에서 아이템을 구매해주세요.\n");
                    return 0;
                }

                int num = 1;
                foreach (Item item in inven)
                {
                    string equipTxt = "- ";

                    if(isManaging)
                    {
                        equipTxt += num.ToString() + " ";
                        if (item.isEquipped)
                        {
                            equipTxt += "[E] ";
                        }
                    }

                    equipTxt += ItemList(item);
                    Console.WriteLine(equipTxt);
                    num++;
                }
                return num;
            }

            public bool isOwned(Item item)
            {
                foreach(Item ownedItem in inven)
                {
                    if(ownedItem.ItemNum == item.ItemNum)
                    {
                        return true;
                    }
                }

                return false;
            }

            public void BuyItem(Item item)
            {
                Console.WriteLine();
                if(!isOwned(item))
                {
                    if (item.ItemGold <= Gold)
                    {
                        Gold -= item.ItemGold;
                        inven.Add(item);
                        Console.WriteLine("'{0}'을 구매했습니다.", item.ItemName);
                    }
                    else
                    {
                        TextColor("Gold가 부족합니다.", ConsoleColor.Red);
                    }
                } else
                {
                    TextColor("이미 구매한 아이템입니다.", ConsoleColor.Red);
                }
            }

            public void SellItem(int num)
            {
                Item item = inven[num - 1];
                if(item.isEquipped)
                {
                    if(item.ItemPart == 0)
                    {
                        DefArmor = null;
                    } else
                    {
                        AtkWeapon = null;
                    }
                }
                Console.WriteLine("{0}'을(를) {1} G 에 판매했습니다.", inven[num - 1].ItemName, inven[num - 1].ItemGold * 0.85);
                Gold += (int) Math.Ceiling(inven[num - 1].ItemGold * 0.85);
                inven.RemoveAt(num - 1);
            }
        }

        public class Monster : ICharacter
        {
            public string Name { get; set; }
            public int AtkPower { get; set; }
            public int DefPower { get; set; }
            public int Health { get; set; }

            public Monster(string name, int atkPower, int defPower, int health)
            {
                Name = name;
                AtkPower = atkPower;
                DefPower = defPower;
                Health = health;
            }

            public void Attack(ICharacter Enemy)
            {
                Random rand = new Random();
                double damage = (AtkPower * rand.Next(50, 151)) * 0.01;
                if(damage - Enemy.DefPower < damage / 2)
                {
                    damage /= 2;
                } else
                {
                    damage -= Enemy.DefPower;
                }
                Enemy.Health -= (int)Math.Round(damage);
                Console.WriteLine("{0} 은(는) {1}의 데미지를 받았다!", Enemy.Name, (int)Math.Round(damage));
            }
        }

        static string ItemList(Item item)
        {
            string atkTxt = "";
            string defTxt = "";

            if (item.ItemAtk != 0)
            {
                atkTxt = "| 공격력 +" + item.ItemAtk + " |";
            }
            if (item.ItemDef != 0)
            {
                defTxt = "| 방어력 +" + item.ItemDef + " |";
            }

            string summary = item.ItemName.PadRight(8) + "   " + atkTxt + defTxt + item.ItemSummary;

            return summary;
        }

        public class Item
        {
            public int ItemNum { get; }
            public string ItemName { get; }
            public int ItemPart { get; }
            public int ItemAtk { get; }
            public int ItemDef { get; }
            public int ItemGold { get; }
            public bool isEquipped { get; set; }
            public string ItemSummary { get; }

            public Item(int itemNum, string itemName, int itemPart, int itemAtk, int itemDef, string itemSummary, int itemGold)
            {
                ItemNum = itemNum;
                ItemName = itemName;
                ItemPart = itemPart; //0은 갑옷 1은 무기
                ItemAtk = itemAtk;
                ItemDef = itemDef;
                ItemGold = itemGold;
                isEquipped = false;
                ItemSummary = itemSummary;
            }
        }

        static void Village(Warrior character, List<Item> shopItem)
        {
            Console.Clear();
            TextColor("\n[광장]", ConsoleColor.Yellow);
            Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.\n");
            Console.WriteLine("     1. 상태 보기");
            Console.WriteLine("     2. 인벤토리");
            Console.WriteLine("     3. 상점");
            Console.WriteLine("     4. 회복하기");
            Console.WriteLine("     5. 던전 입장");
            Console.WriteLine("     6. 저장하기");
            Console.WriteLine();

            int cmd = CheckAction("어떤 활동을 하시겠습니까?", 1, 6);

            switch (cmd)
            {
                case 1:
                    //상태보기
                    Console.Clear();
                    character.ShowStatus();
                    cmd = CheckAction("\n0. 나가기\n\n원하시는 행동을 입력해주세요.", 0, 0);
                    if(cmd == 0)
                    {
                        Village(character, shopItem);
                    }
                    break;
                case 2:
                    //인벤토리
                    ManageInven(character, shopItem);
                    break;
                case 3:
                    //상점
                    Store(character, shopItem, true, 0);
                    break;
                case 4:
                    //병원
                    Hospital(character, shopItem);
                    break;
                case 5:
                    //던전
                    Dungeon(character, shopItem);
                    break;
                case 6:
                    SaveJson(character, shopItem);
                    break;
            }
        }
        static void ManageInven(Warrior character, List<Item> shopItem)
        {
            Console.Clear();
            TextColor("\n인벤토리", ConsoleColor.Yellow);
            Console.WriteLine("보유 중인 아이템을 관리할 수 있습니다.\n");

            character.ShowInven(false);

            while(true)
            {
                Console.WriteLine("\n1. 장착 관리");
                Console.WriteLine("0. 나가기\n");
                int cmd = CheckAction("원하는 행동을 입력해주세요.", 0, 1);

                if (cmd == 0)
                {
                    //나가기
                    Village(character, shopItem);
                    break;
                }
                else
                {
                    // 장착 관리
                    int max = character.ShowInven(true);
                    if(max == 0)
                    {
                        int command = CheckAction("0. 나가기", 0, 0);
                        if(command == 0) //불필요한가?
                        {
                            Village(character, shopItem);
                        }
                        break;
                    } else
                    {
                        int itemNum = CheckAction("\n장착/장착 해제할 아이템을 선택해주세요.", 1, max);
                        //아이템 장착 character.EquipItem() 아마 번호 파라미터로넣을듯?
                        character.EquipItem(itemNum);
                        continue;
                    }
                }
            }
            
        }
        static void Store(Warrior character, List<Item> shopItem, bool isFirst, int cmd)
        {
            Console.Clear();
            TextColor("\n[상점]", ConsoleColor.Yellow);
            Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.");

            Console.WriteLine("\n[보유 골드]");
            TextColor(character.Gold + " G\n", ConsoleColor.Yellow);

            if(cmd != 2)
            {
                int cnt = 1;
                foreach (Item item in shopItem)
                {
                    string equipTxt = "- " + cnt + " ";

                    equipTxt += ItemList(item);
                    //구매한 아이템이라면 구매완료로 분리
                    if (character.isOwned(item))
                    {
                        equipTxt += " | 구매 완료";
                        TextColor(equipTxt, ConsoleColor.DarkGray);
                    }
                    else
                    {
                        equipTxt += " | " + item.ItemGold.ToString() + " G";
                        Console.WriteLine(equipTxt);
                    }
                    cnt++;
                }
            }

            if(isFirst)
            {
                Console.WriteLine("\n1. 아이템 구매");
                Console.WriteLine("2. 아이템 판매");
                Console.WriteLine("0. 나가기\n");

                cmd = CheckAction("원하는 행동을 입력해주세요.", 0, 2);
                if (cmd == 0)
                {
                    Village(character, shopItem);
                }
            }

            if(cmd == 1)
            {
                int itemNum = CheckAction("\n구매할 아이템 번호를 선택해주세요.", 1, shopItem.Count);
                character.BuyItem(shopItem[itemNum - 1]);

                Console.WriteLine("\n1. 아이템 구매 계속하기");
                Console.WriteLine("0. 나가기\n");
                cmd = CheckAction("원하는 행동을 입력해주세요.", 0, 1);

                if (cmd == 0)
                {
                    Village(character, shopItem);
                }
                else if (cmd == 1)
                {
                    Store(character, shopItem, false, 1);
                }
            } else if(cmd == 2)
            {
                int num = character.ShowInven(true);
                int itemNum = CheckAction("\n어떤 것을 판매하시겠습니까?", 1, num - 1);
                character.SellItem(itemNum);

                Console.WriteLine("\n1. 아이템 판매 계속하기");
                Console.WriteLine("0. 나가기\n");
                cmd = CheckAction("원하는 행동을 입력해주세요.", 0, 1);
                if (cmd == 0)
                {
                    Village(character, shopItem);
                }
                else if (cmd == 1)
                {
                    Store(character, shopItem, false, 2);
                }
            }
        }
        static void Hospital(Warrior character, List<Item> shopItem)
        {
            TextColor("\n[병원]", ConsoleColor.Yellow);
            Console.WriteLine("체력을 회복할 수 있습니다.\n");

            Console.WriteLine("현재 체력 : " + character.Health + " / " + character.maxHP);

            while(true)
            {
                Console.WriteLine("\n1. 회복하기");
                Console.WriteLine("0. 나가기\n");
                int cmd = CheckAction("어떤 행동을 하시겠습니까?", 0, 1);

                if (cmd == 0)
                {
                    Village(character, shopItem);
                }
                else
                {
                    if (character.Health == character.maxHP)
                    {
                        Console.WriteLine("이미 체력이 가득 찬 상태입니다.");
                        continue;
                    } else if(character.Gold >= 500)
                    {
                        character.Heal();
                        character.Gold -= 500;
                        Console.WriteLine("500 G 를 지불하였습니다.");
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("골드가 부족합니다.");
                        continue;
                    }
                }
            }
        }
        static int CheckAction(string start, int min, int max)
        {
            while(true)
            {
                Console.WriteLine(start);
                Console.Write(">> ");

                int ans;
                if(int.TryParse(Console.ReadLine(), out ans))
                {
                    if(ans >= min && ans <= max)
                    {
                        return ans;
                    }
                }
                Console.WriteLine("\n잘못된 입력입니다. 다시 선택해주십시오.");
            }
        }
        static void TextColor(string text, ConsoleColor clr)
        {
            Console.ForegroundColor = clr;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        static void Dungeon(Warrior character, List<Item> shopItem)
        {
            TextColor("\n[던전]", ConsoleColor.Yellow);

            Console.WriteLine("\n1. 훈련 던전     | 방어력 5 이상 권장");
            Console.WriteLine("2. 일반 던전     | 방어력 11 이상 권장");
            Console.WriteLine("3. 드래곤 던전   | 방어력 20 이상 권장");
            Console.WriteLine("0. 나가기\n");

            int cmd = CheckAction("어느 던전을 들어가시겠습니까?", 0, 3);
            bool isWin = false;
            Console.Clear();
            switch(cmd)
            {
                case 0:
                    Village(character, shopItem);
                    break;
                case 1:
                    Monster troll = new Monster("트롤", 4, 2, 30);
                    isWin = Battle(character, troll);
                    break;
                case 2:
                    Monster goblin = new Monster("고블린", 8, 5, 100);
                    isWin = Battle(character, goblin);
                    break;
                case 3:
                    Monster dragon = new Monster("드래곤", 30, 20, 300);
                    isWin = Battle(character, dragon);
                    break;
            }

            if(isWin == true)
            {
                character.LevelUP();
                AddStoreList(shopItem, character.Level);
            }

            Village(character, shopItem);
        }

        static bool Battle(Warrior character, ICharacter enemy)
        {
            int turn = 1;
            while(true)
            {
                TextColor("\n[Turn " + turn + "]", ConsoleColor.Yellow);
                Console.WriteLine(enemy.Name + " / 남은 체력 : (" + enemy.Health + ")\n");

                Console.WriteLine("1. 공격");
                Console.WriteLine("2. 내 상태 확인");
                Console.WriteLine("0. 도망가기\n");

                int cmd = CheckAction("무엇을 하시겠습니까?", 0, 2);
                if(cmd == 0)
                {
                    return false;
                } else if(cmd == 1)
                {
                    turn++;
                    character.Attack(enemy);

                    if(enemy.Health <= 0)
                    {
                        Console.WriteLine(enemy.Name + "을(를) 물리쳤습니다!");
                        return true;
                    }

                    enemy.Attack(character);

                    if(character.Health <= 0)
                    {
                        Console.WriteLine(character.Name + "님이 전투불능 상태가 되었습니다.");
                        return false;
                        break;
                    }
                } else if(cmd == 2)
                {
                    character.ShowStatus();
                }
            }
        }

        static void SaveJson(Warrior character, List<Item> shoplist)
        {
            Console.Clear();
            TextColor("[저장하기]\n", ConsoleColor.Yellow);

            string filepath = "character.json";
            string json = JsonConvert.SerializeObject(character);
            File.WriteAllText(filepath, json);

            filepath = "shoplist.json";
            json = JsonConvert.SerializeObject(shoplist);
            File.WriteAllText(filepath, json);

            Console.WriteLine("\n저장했습니다.\n");
            CheckAction("0. 나가기\n", 0, 0);
            Village(character, shoplist);
        }

        static Warrior LoadCharacter()
        {
            string json = File.ReadAllText("character.json");
            Warrior character = JsonConvert.DeserializeObject<Warrior>(json);

            return character;
        }

        static List<Item> LoadShop()
        {
            string json = File.ReadAllText("shoplist.json");
            List<Item> shopList = JsonConvert.DeserializeObject<List<Item>>(json);

            return shopList;
        }
        static Warrior MakeCharacter()
        {
            Warrior character;

            string name;

            while (true)
            {
                Console.WriteLine("당신의 이름은 무엇입니까?");
                name = Console.ReadLine();
                Console.WriteLine("\n '{0}' 이 당신의 이름이 맞습니까?\n", name);
                Console.WriteLine("     1. 맞습니다.");
                Console.WriteLine("     2. 아닙니다.\n");
                Console.Write(">> ");
                string ans = Console.ReadLine();

                if (ans == "1")
                {
                    Console.WriteLine("{0}님 환영합니다.\n", name);
                    break;
                }
                else
                {
                    Console.WriteLine("이름은 바꿀 수 없으니 신중히 입력하십시오.\n");
                    continue;
                }
            }
            Console.WriteLine("                     직업을 선택해주세요.\n");

            Console.WriteLine("         |   1. 전사    |   2. 용병    |  3. 마법사  ");
            Console.WriteLine("---------|--------------|--------------|--------------");
            Console.WriteLine(" 공격력  |      10      |      8       |      12  ");
            Console.WriteLine("---------|--------------|--------------|--------------");
            Console.WriteLine(" 방어력  |      7       |      10      |      5  ");
            Console.WriteLine("---------|--------------|--------------|--------------");
            Console.WriteLine(" 체  력  |     100      |      80      |      50  ");
            Console.WriteLine("---------|--------------|--------------|--------------");
            Console.WriteLine(" 골  드  |     1500     |     1200     |     2000  ");

            int job = CheckAction("", 1, 3);
            character = new Warrior(name, job);

            Console.WriteLine("\n{0} {1}님, 스파르타 마을에 오신걸 환영합니다.", character.Job, character.Name);

            return character;
        }

        static List<Item> AddStoreList(List<Item> shopItem, int level)
        {
            // 레벨업 후 숍 방문시 실행 필요
            Item[] lv1 = { new Item(0, "수련자 갑옷", 0, 0, 5, " 초보자를 위한 수련에 도움을 주는 갑옷.", 500),
                           new Item(1, "낡은 검", 1, 2, 0, " 쉽게 부서질 것 같은 낡은 검.", 300),
                           new Item(2, "청동 도끼", 1, 4, 0, " 방금까지도 나무를 벤 것 같은 흔한 도끼.", 900)};

            Item[] lv2 = { new Item(3,"무쇠 갑옷", 0, 0, 10, " 숙련자가 만든 튼튼한 갑옷.", 1200),
                           new Item(4,"가벼운 은검", 1, 7, 0, " 휴대하기 좋은 날카로운 은검.", 1200)};

            Item[] lv3 = { new Item(5,"장인의 갑옷", 0, 0, 15, " 장인이 만든 갑옷. 1년에 단 1개만 제작된다고 한다.", 2000),
                           new Item(6,"장인의 검", 1, 10, 0, " 장인이 만든 검. 스치기만 해도 치명상을 입는다.", 1600),
                           new Item(7,"장인의 활", 1, 7, 3, " 장인이 만든 활. 먼 거리에서도 화살이 올곧게 날아간다.", 1600)};

            //전설의 전용 무기
            Item[] lv5 = { new Item(8,"이카본의 쌍검", 2, 20, 10, " 초대 군주 이카본이 기사단장에게 수여했다고 전해지는 전설의 검. 기사만이 사용할 수 있다.", 5000),
                           new Item(9,"용병왕의 대검", 3, 15, 15, " 사라진 용병왕의 대검. 용병이 아닌 자는 들 수 없다.", 5000),
                           new Item(10,"아나로즈의 망토", 4, 10, 10, " 수천년을 산 마녀 아나로즈의 망토. 마법사가 아닌 자는 그 용도를 알 수 없다.", 5000)};

            if (level == 1)
            {
                for (int i = 0; i < lv1.Length; i++)
                {
                    shopItem.Add(lv1[i]);
                }
            }
            else if (level == 2)
            {
                for (int i = 0; i < lv2.Length; i++)
                {
                    shopItem.Add(lv2[i]);
                }
            }
            else if (level == 3)
            {
                for (int i = 0; i < lv3.Length; i++)
                {
                    shopItem.Add(lv3[i]);
                }
            }
            else if (level == 5)
            {
                for (int i = 0; i < lv5.Length; i++)
                {
                    shopItem.Add(lv5[i]);
                }
            }

            return shopItem;
        } 

        static void Main(string[] args)
        {
            Warrior character;
            List<Item> shopItem = new List<Item>();

            //게임 시작 화면
            Console.WriteLine("    _______  .______          ___       _______   ______   .__   __.   \r\n   |       \\ |   _  \\        /   \\     /  _____| /  __  \\  |  \\ |  |   \r\n   |  .--.  ||  |_)  |      /  ^  \\   |  |  __  |  |  |  | |   \\|  |   \r\n   |  |  |  ||      /      /  /_\\  \\  |  | |_ | |  |  |  | |  . `  |   \r\n   |  '--'  ||  |\\  \\----./  _____  \\ |  |__| | |  `--'  | |  |\\   |   \r\n   |_______/ | _| `._____/__/     \\__\\ \\______|  \\______/  |__| \\__|   \r\n                                                                       \r\n _______   __    __  .__   __.   _______  _______   ______   .__   __. \r\n|       \\ |  |  |  | |  \\ |  |  /  _____||   ____| /  __  \\  |  \\ |  | \r\n|  .--.  ||  |  |  | |   \\|  | |  |  __  |  |__   |  |  |  | |   \\|  | \r\n|  |  |  ||  |  |  | |  . `  | |  | |_ | |   __|  |  |  |  | |  . `  | \r\n|  '--'  ||  `--'  | |  |\\   | |  |__| | |  |____ |  `--'  | |  |\\   | \r\n|_______/  \\______/  |__| \\__|  \\______| |_______| \\______/  |__| \\__| \r\n                                                                       ");

            CheckAction("PRESS 0 TO START", 0, 0);

            //캐릭터 생성
            if(File.Exists("character.json") && File.Exists("shoplist.json"))
            {
                character = LoadCharacter();
                shopItem = LoadShop();
            } else
            {
                character = MakeCharacter();
                shopItem = AddStoreList(shopItem, character.Level);
            }

            Village(character, shopItem);
        }
    }
}
