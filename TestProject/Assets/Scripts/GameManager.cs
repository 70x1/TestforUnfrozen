using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct Warrior // Структура для определения и хранения базовых свойств воинов
    {
        public string color; // цвет воина
        public int number; // ячейка воина в армии
        public int iniciative; // инициатива воина
        public int speed;  // скорость воина
    }

    public List<Warrior> warriorsBaseList; // Массив структур воинов по умолчанию, контроллируется из инспектора

    public GameObject redArmyList; // Родитель в иерархии для красных воинов
    public GameObject blueArmyList; // Родитель в иерархии для синих воинов

    public Sprite redBg; // Картинка ячейки для красных воинов
    public Sprite blueBg; // для синих
    
    public GameObject CloseRaycast;// Если включена автосимуляция закрывает доступ к кнопка действия
    public Text Whowins; //Отображает победителя

    public GameObject RoundNext; // Плашка следующего раунда
    Text Round; //Текст номера на плашке следующего раунда
    public Text Globalturntext; // Отображает общий номер хода
    int Globalturn;//

    public GameObject WarriorPrefab; // Префаб воина для создания объектов

    public GameObject CreateGameMenu;
    public GameObject BattleGameMenu;

    public GameObject Battlefield;// Родительский объект поля боя

    int killedWarriors;

    int roundNumber;
    int turnNumber;

    public Toggle Autosim; // Тоггл функции автосимуляции

    public Text roundNumtxt;
    public Text turnNumtxt;
    public class warriorInGame // Класс для определения свойств воинов, которая будет использована при создании игры, сортировке и т.д.
    {
        public string color; // цвет воина
        public int number; // ячейка воина в армии
        public int iniciative; // инициатива воина
        public int speed;  // скорость воина

        public GameObject WarriorObject; // ссылка на объект воина

        public Image warriorBg; //для изменения цвета ячейки воина

        public Text numberLabel; // отображает номер ячейки воина в армии

        public Text iniciativeInput; // количество инициативы 

        public Text speedInput; // количество скорости 

        public bool killed;

        public bool turned;

    }
    public List<warriorInGame> WarriorsList = new List<warriorInGame>(); // Лист воинов

    private void Awake()
    {
        CreateStartList();
        Round = RoundNext.transform.GetChild(0).GetComponent<Text>();
    }
    void Start()
    {

    }
    private void Update()
    {

    }
    private void CreateStartList()
    {
        for (int i = 0; i < warriorsBaseList.Count; i++) // Цикл для создания базового меню воинов, присваивания значений по умолчанию и отображению параметров воина в игровых компонентах
        {
            WarriorsList.Add(new warriorInGame());
            WarriorsList[i].color = warriorsBaseList[i].color;
            WarriorsList[i].number = warriorsBaseList[i].number;
            WarriorsList[i].iniciative = warriorsBaseList[i].iniciative;
            WarriorsList[i].speed = warriorsBaseList[i].speed; // для начала присвоим базовые параметры воину, в последствии игрок их поменяет если захочет

            if (i % 2 == 0) // определение армии воина исходя из данных введенны в инспекторе как базовых (воины были добавлены через одного сначала красный, потом синий)
            {
                WarriorsList[i].WarriorObject = Instantiate(WarriorPrefab, redArmyList.transform); // создание объекта воина и запись его в поле переменной внутри класса
                WarriorsList[i].numberLabel = WarriorsList[i].WarriorObject.transform.GetChild(0).GetComponent<Text>();
                WarriorsList[i].numberLabel.text = "K" + WarriorsList[i].number.ToString();//отображает номер воина и цвет его армии
                WarriorsList[i].warriorBg = WarriorsList[i].WarriorObject.GetComponent<Image>();
                WarriorsList[i].warriorBg.sprite = redBg; //определяет цвет воина
            }
            else
            {
                WarriorsList[i].WarriorObject = Instantiate(WarriorPrefab, blueArmyList.transform);// Создаем воина из префаба
                WarriorsList[i].numberLabel = WarriorsList[i].WarriorObject.transform.GetChild(0).GetComponent<Text>();//Присваиваем ссылки на ui элементы
                WarriorsList[i].numberLabel.text = "C" + WarriorsList[i].number.ToString();//..
                WarriorsList[i].warriorBg = WarriorsList[i].WarriorObject.GetComponent<Image>();
                WarriorsList[i].warriorBg.sprite = blueBg;//..
            }



            WarriorsList[i].iniciativeInput = WarriorsList[i].WarriorObject.transform.GetChild(1).GetChild(1).GetComponent<Text>();
            WarriorsList[i].speedInput = WarriorsList[i].WarriorObject.transform.GetChild(2).GetChild(1).GetComponent<Text>();
            WarriorsList[i].iniciativeInput.text = warriorsBaseList[i].iniciative.ToString();
            WarriorsList[i].speedInput.text = warriorsBaseList[i].speed.ToString();


            WarriorsList[i].turned = false; //Воин помечается как походивший с помощью этой переменной

        }

    }

    public void CreateGame()// Создает игру и сортирует массив воинов
    {

        roundNumber = 1;
        turnNumber = 1;
        Globalturn = 1;//Присваивание начальых значений

        Round.text = roundNumber.ToString();
      
        Sort();//Сортировка
        for (int i = 0; i < WarriorsList.Count; i++)
        {
            WarriorsList[i].WarriorObject.transform.SetParent(Battlefield.transform);//Перенос воинов на поле сражения (контент у скроллвью)

        }
        Refreshlabels();
        CreateGameMenu.SetActive(false);
        BattleGameMenu.SetActive(true);
        if (Autosim.isOn)
        
            AutoSimulation();
        
    }
    void Sort()
    {

        string prioritycolor;// Определяет приоритетный цвет в раунде
        if (roundNumber % 2 == 0)
            prioritycolor = "blue";
        else
            prioritycolor = "red";

        warriorInGame buffer;// Будем использовать для перестановок элементов массива

        for (int i = 1; i < WarriorsList.Count - (turnNumber - 1); i++)//Сортировка вставками подмассива непоходивших воинов
        {
            int j = i;


            while (j > 0 && WarriorsList[j].iniciative > WarriorsList[j - 1].iniciative)//сортировка по инициативе
            {
                buffer = WarriorsList[j];
                WarriorsList[j] = WarriorsList[j - 1];
                WarriorsList[j - 1] = buffer;
                j--;

            }
            while (j > 0 && WarriorsList[j].speed > WarriorsList[j - 1].speed && WarriorsList[j].iniciative == WarriorsList[j - 1].iniciative)//сортируем по скорости если есть элементы с равной инициативой
            {
                buffer = WarriorsList[j];
                WarriorsList[j] = WarriorsList[j - 1];
                WarriorsList[j - 1] = buffer;
                j--;

            }
            if (WarriorsList[j].color == prioritycolor)//Сортируем по цвету если выбранный элемент приоритетного цвета и совпадают скорость и инициатива
            {

                while (j > 0 && WarriorsList[j].speed == WarriorsList[j - 1].speed && WarriorsList[j].iniciative == WarriorsList[j - 1].iniciative && WarriorsList[j - 1].color != prioritycolor)// Выносим элемента к началу массива до того как он встретит приоритетный цвет
                {
                    buffer = WarriorsList[j];
                    WarriorsList[j] = WarriorsList[j - 1];
                    WarriorsList[j - 1] = buffer;
                    j--;

                }
                while (j > 0 && WarriorsList[j].speed == WarriorsList[j - 1].speed && WarriorsList[j].iniciative == WarriorsList[j - 1].iniciative && WarriorsList[j].number < WarriorsList[j - 1].number)// Теперь отсортируем по порядковому номеру в приоритетном цвете
                {
                    buffer = WarriorsList[j];
                    WarriorsList[j] = WarriorsList[j - 1];
                    WarriorsList[j - 1] = buffer;
                    j--;

                }

            }
            else
            {
                while (j > 0 && WarriorsList[j].speed == WarriorsList[j - 1].speed && WarriorsList[j].iniciative == WarriorsList[j - 1].iniciative && WarriorsList[j - 1].color != prioritycolor && WarriorsList[j].number < WarriorsList[j - 1].number)//Если цвет неприоритетный элементы буду сортировать по порядковому номеру ближе к правому краю подмассива с одинаковыми свойствами
                {
                    buffer = WarriorsList[j];
                    WarriorsList[j] = WarriorsList[j - 1];
                    WarriorsList[j - 1] = buffer;
                    j--;

                }
            }




        }

        if (roundNumber % 2 == 0)// Код ниже используется для сортировки подмассива походивших воинов для составления порядка хода на следующий раунд
            prioritycolor = "red";
        else
            prioritycolor = "blue";

        for (int i = WarriorsList.Count - (turnNumber-2); i < WarriorsList.Count; i++)
        {
            int j = i;
            Debug.Log(WarriorsList.Count - (turnNumber - 2));

            while (j > WarriorsList.Count - (turnNumber - 1) && WarriorsList[j].iniciative > WarriorsList[j - 1].iniciative)
            {
                buffer = WarriorsList[j];
                WarriorsList[j] = WarriorsList[j - 1];
                WarriorsList[j - 1] = buffer;
                j--;

            }
            while (j > WarriorsList.Count - (turnNumber - 1) && WarriorsList[j].speed > WarriorsList[j - 1].speed && WarriorsList[j].iniciative == WarriorsList[j - 1].iniciative)
            {
                buffer = WarriorsList[j];
                WarriorsList[j] = WarriorsList[j - 1];
                WarriorsList[j - 1] = buffer;
                j--;

            }
            if (WarriorsList[j].color == prioritycolor)
            {

                while (j > WarriorsList.Count - (turnNumber - 1) && WarriorsList[j].speed == WarriorsList[j - 1].speed && WarriorsList[j].iniciative == WarriorsList[j - 1].iniciative && WarriorsList[j - 1].color != prioritycolor)
                {
                    buffer = WarriorsList[j];
                    WarriorsList[j] = WarriorsList[j - 1];
                    WarriorsList[j - 1] = buffer;
                    j--;

                }
                while (j > WarriorsList.Count - (turnNumber - 1) && WarriorsList[j].speed == WarriorsList[j - 1].speed && WarriorsList[j].iniciative == WarriorsList[j - 1].iniciative && WarriorsList[j].number < WarriorsList[j - 1].number)
                {
                    buffer = WarriorsList[j];
                    WarriorsList[j] = WarriorsList[j - 1];
                    WarriorsList[j - 1] = buffer;
                    j--;

                }

            }
            else
            {
                while (j > WarriorsList.Count - (turnNumber - 1) && WarriorsList[j].speed == WarriorsList[j - 1].speed && WarriorsList[j].iniciative == WarriorsList[j - 1].iniciative && WarriorsList[j - 1].color != prioritycolor && WarriorsList[j].number < WarriorsList[j - 1].number)
                {
                    buffer = WarriorsList[j];
                    WarriorsList[j] = WarriorsList[j - 1];
                    WarriorsList[j - 1] = buffer;
                    j--;

                }
            }
        }
        //Debug.Log(WarriorsList.Count);

    }
    public void KillNext()//Убивает следующего воина в цепочке и пропускает ход
    {
        Destroy(WarriorsList[1].WarriorObject);
        WarriorsList.Remove(WarriorsList[1]);
        Turn();
    }

    public void Turn()//Функция для пропуска хода
    {
        WarriorsList[0].turned = true;//Воин походил
        for (int i = 0; i < WarriorsList.Count - (turnNumber); i++)
        {
            warriorInGame buffer;
            buffer = WarriorsList[i];
            WarriorsList[i] = WarriorsList[i + 1];
            WarriorsList[i + 1] = buffer;
        }//Ставим походившего в конец массива
        turnNumber++;//..
        
        if (turnNumber >= WarriorsList.Count + 1)
        {
            turnNumber = 1;
            roundNumber++;

            for (int i = 0; i < WarriorsList.Count; i++)
                WarriorsList[i].turned = false;//конец раунда сброс походивших воинов
        }
        Sort();
        // сортировка воинов после действия
        int RedLeft=0;
        int BlueLeft=0;
        for (int i = 0; i < WarriorsList.Count; i++)
        {
            WarriorsList[i].WarriorObject.transform.SetSiblingIndex(i);//сортировка отображения воинов на поле битвы
            if (WarriorsList[i].color == "red")
                RedLeft++;
            else if (WarriorsList[i].color == "blue")
                BlueLeft++;


        }
        if (turnNumber > 1)
        {
            RoundNext.transform.SetSiblingIndex(WarriorsList.Count - (turnNumber - 1));
            Round.text = (roundNumber + 1).ToString();//Управляет плашкой с номером раунда 
        }
        else

            RoundNext.transform.SetSiblingIndex(0);
      

        Globalturn++;//
        Refreshlabels();// Обновляем данные на ui элементах

        if (BlueLeft == 0)// Проверим наличие победителя
        {
            CloseRaycast.SetActive(true);
            Whowins.text = "Красные победили";
            CancelInvoke();
        }
        else if (RedLeft == 0)
        {
            CloseRaycast.SetActive(true);
            Whowins.text = "Cиние победили";
            CancelInvoke();
            }

    }
    void Refreshlabels()//Функция для обновления данных в элементах UI
    {
        Globalturntext.text = "ОБЩИЙ НОМЕР ХОДА " + Globalturn.ToString();
        roundNumtxt.text = "НОМЕР РАУНДА - " + roundNumber.ToString();
        turnNumtxt.text = "НОМЕР ХОДА В РАУНДЕ- " + turnNumber.ToString() + "\n" + "ХОДИТ - " + WarriorsList[0].color + WarriorsList[0].number.ToString();
    }
    public void ExitApp()//..
    {
        Application.Quit();
    }
    public void ReloadScene()
        {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    public void AutoSimulation()
    {
        CloseRaycast.SetActive(true);
        InvokeRepeating("ActionAuto", 1,1);
    }
    void ActionAuto()//Убивает следующего если враг, если свой пропускает ход
    {
        if (WarriorsList[0].color != WarriorsList[1].color)
        {
            KillNext();
        }
        else
            Turn();
    }    
}
