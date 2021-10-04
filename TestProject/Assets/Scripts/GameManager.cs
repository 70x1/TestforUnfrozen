using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct Warrior // ��������� ��� ����������� � �������� ������� ������� ������
    {
        public string color; // ���� �����
        public int number; // ������ ����� � �����
        public int iniciative; // ���������� �����
        public int speed;  // �������� �����
    }

    public List<Warrior> warriorsBaseList; // ������ �������� ������ �� ���������, ��������������� �� ����������

    public GameObject redArmyList; // �������� � �������� ��� ������� ������
    public GameObject blueArmyList; // �������� � �������� ��� ����� ������

    public Sprite redBg; // �������� ������ ��� ������� ������
    public Sprite blueBg; // ��� �����
    
    public GameObject CloseRaycast;// ���� �������� ������������� ��������� ������ � ������ ��������
    public Text Whowins; //���������� ����������

    public GameObject RoundNext; // ������ ���������� ������
    Text Round; //����� ������ �� ������ ���������� ������
    public Text Globalturntext; // ���������� ����� ����� ����
    int Globalturn;//

    public GameObject WarriorPrefab; // ������ ����� ��� �������� ��������

    public GameObject CreateGameMenu;
    public GameObject BattleGameMenu;

    public GameObject Battlefield;// ������������ ������ ���� ���

    int killedWarriors;

    int roundNumber;
    int turnNumber;

    public Toggle Autosim; // ����� ������� �������������

    public Text roundNumtxt;
    public Text turnNumtxt;
    public class warriorInGame // ����� ��� ����������� ������� ������, ������� ����� ������������ ��� �������� ����, ���������� � �.�.
    {
        public string color; // ���� �����
        public int number; // ������ ����� � �����
        public int iniciative; // ���������� �����
        public int speed;  // �������� �����

        public GameObject WarriorObject; // ������ �� ������ �����

        public Image warriorBg; //��� ��������� ����� ������ �����

        public Text numberLabel; // ���������� ����� ������ ����� � �����

        public Text iniciativeInput; // ���������� ���������� 

        public Text speedInput; // ���������� �������� 

        public bool killed;

        public bool turned;

    }
    public List<warriorInGame> WarriorsList = new List<warriorInGame>(); // ���� ������

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
        for (int i = 0; i < warriorsBaseList.Count; i++) // ���� ��� �������� �������� ���� ������, ������������ �������� �� ��������� � ����������� ���������� ����� � ������� �����������
        {
            WarriorsList.Add(new warriorInGame());
            WarriorsList[i].color = warriorsBaseList[i].color;
            WarriorsList[i].number = warriorsBaseList[i].number;
            WarriorsList[i].iniciative = warriorsBaseList[i].iniciative;
            WarriorsList[i].speed = warriorsBaseList[i].speed; // ��� ������ �������� ������� ��������� �����, � ����������� ����� �� �������� ���� �������

            if (i % 2 == 0) // ����������� ����� ����� ������ �� ������ �������� � ���������� ��� ������� (����� ���� ��������� ����� ������ ������� �������, ����� �����)
            {
                WarriorsList[i].WarriorObject = Instantiate(WarriorPrefab, redArmyList.transform); // �������� ������� ����� � ������ ��� � ���� ���������� ������ ������
                WarriorsList[i].numberLabel = WarriorsList[i].WarriorObject.transform.GetChild(0).GetComponent<Text>();
                WarriorsList[i].numberLabel.text = "K" + WarriorsList[i].number.ToString();//���������� ����� ����� � ���� ��� �����
                WarriorsList[i].warriorBg = WarriorsList[i].WarriorObject.GetComponent<Image>();
                WarriorsList[i].warriorBg.sprite = redBg; //���������� ���� �����
            }
            else
            {
                WarriorsList[i].WarriorObject = Instantiate(WarriorPrefab, blueArmyList.transform);// ������� ����� �� �������
                WarriorsList[i].numberLabel = WarriorsList[i].WarriorObject.transform.GetChild(0).GetComponent<Text>();//����������� ������ �� ui ��������
                WarriorsList[i].numberLabel.text = "C" + WarriorsList[i].number.ToString();//..
                WarriorsList[i].warriorBg = WarriorsList[i].WarriorObject.GetComponent<Image>();
                WarriorsList[i].warriorBg.sprite = blueBg;//..
            }



            WarriorsList[i].iniciativeInput = WarriorsList[i].WarriorObject.transform.GetChild(1).GetChild(1).GetComponent<Text>();
            WarriorsList[i].speedInput = WarriorsList[i].WarriorObject.transform.GetChild(2).GetChild(1).GetComponent<Text>();
            WarriorsList[i].iniciativeInput.text = warriorsBaseList[i].iniciative.ToString();
            WarriorsList[i].speedInput.text = warriorsBaseList[i].speed.ToString();


            WarriorsList[i].turned = false; //���� ���������� ��� ���������� � ������� ���� ����������

        }

    }

    public void CreateGame()// ������� ���� � ��������� ������ ������
    {

        roundNumber = 1;
        turnNumber = 1;
        Globalturn = 1;//������������ �������� ��������

        Round.text = roundNumber.ToString();
      
        Sort();//����������
        for (int i = 0; i < WarriorsList.Count; i++)
        {
            WarriorsList[i].WarriorObject.transform.SetParent(Battlefield.transform);//������� ������ �� ���� �������� (������� � ���������)

        }
        Refreshlabels();
        CreateGameMenu.SetActive(false);
        BattleGameMenu.SetActive(true);
        if (Autosim.isOn)
        
            AutoSimulation();
        
    }
    void Sort()
    {

        string prioritycolor;// ���������� ������������ ���� � ������
        if (roundNumber % 2 == 0)
            prioritycolor = "blue";
        else
            prioritycolor = "red";

        warriorInGame buffer;// ����� ������������ ��� ������������ ��������� �������

        for (int i = 1; i < WarriorsList.Count - (turnNumber - 1); i++)//���������� ��������� ���������� ������������ ������
        {
            int j = i;


            while (j > 0 && WarriorsList[j].iniciative > WarriorsList[j - 1].iniciative)//���������� �� ����������
            {
                buffer = WarriorsList[j];
                WarriorsList[j] = WarriorsList[j - 1];
                WarriorsList[j - 1] = buffer;
                j--;

            }
            while (j > 0 && WarriorsList[j].speed > WarriorsList[j - 1].speed && WarriorsList[j].iniciative == WarriorsList[j - 1].iniciative)//��������� �� �������� ���� ���� �������� � ������ �����������
            {
                buffer = WarriorsList[j];
                WarriorsList[j] = WarriorsList[j - 1];
                WarriorsList[j - 1] = buffer;
                j--;

            }
            if (WarriorsList[j].color == prioritycolor)//��������� �� ����� ���� ��������� ������� ������������� ����� � ��������� �������� � ����������
            {

                while (j > 0 && WarriorsList[j].speed == WarriorsList[j - 1].speed && WarriorsList[j].iniciative == WarriorsList[j - 1].iniciative && WarriorsList[j - 1].color != prioritycolor)// ������� �������� � ������ ������� �� ���� ��� �� �������� ������������ ����
                {
                    buffer = WarriorsList[j];
                    WarriorsList[j] = WarriorsList[j - 1];
                    WarriorsList[j - 1] = buffer;
                    j--;

                }
                while (j > 0 && WarriorsList[j].speed == WarriorsList[j - 1].speed && WarriorsList[j].iniciative == WarriorsList[j - 1].iniciative && WarriorsList[j].number < WarriorsList[j - 1].number)// ������ ����������� �� ����������� ������ � ������������ �����
                {
                    buffer = WarriorsList[j];
                    WarriorsList[j] = WarriorsList[j - 1];
                    WarriorsList[j - 1] = buffer;
                    j--;

                }

            }
            else
            {
                while (j > 0 && WarriorsList[j].speed == WarriorsList[j - 1].speed && WarriorsList[j].iniciative == WarriorsList[j - 1].iniciative && WarriorsList[j - 1].color != prioritycolor && WarriorsList[j].number < WarriorsList[j - 1].number)//���� ���� �������������� �������� ���� ����������� �� ����������� ������ ����� � ������� ���� ���������� � ����������� ����������
                {
                    buffer = WarriorsList[j];
                    WarriorsList[j] = WarriorsList[j - 1];
                    WarriorsList[j - 1] = buffer;
                    j--;

                }
            }




        }

        if (roundNumber % 2 == 0)// ��� ���� ������������ ��� ���������� ���������� ���������� ������ ��� ����������� ������� ���� �� ��������� �����
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
    public void KillNext()//������� ���������� ����� � ������� � ���������� ���
    {
        Destroy(WarriorsList[1].WarriorObject);
        WarriorsList.Remove(WarriorsList[1]);
        Turn();
    }

    public void Turn()//������� ��� �������� ����
    {
        WarriorsList[0].turned = true;//���� �������
        for (int i = 0; i < WarriorsList.Count - (turnNumber); i++)
        {
            warriorInGame buffer;
            buffer = WarriorsList[i];
            WarriorsList[i] = WarriorsList[i + 1];
            WarriorsList[i + 1] = buffer;
        }//������ ����������� � ����� �������
        turnNumber++;//..
        
        if (turnNumber >= WarriorsList.Count + 1)
        {
            turnNumber = 1;
            roundNumber++;

            for (int i = 0; i < WarriorsList.Count; i++)
                WarriorsList[i].turned = false;//����� ������ ����� ���������� ������
        }
        Sort();
        // ���������� ������ ����� ��������
        int RedLeft=0;
        int BlueLeft=0;
        for (int i = 0; i < WarriorsList.Count; i++)
        {
            WarriorsList[i].WarriorObject.transform.SetSiblingIndex(i);//���������� ����������� ������ �� ���� �����
            if (WarriorsList[i].color == "red")
                RedLeft++;
            else if (WarriorsList[i].color == "blue")
                BlueLeft++;


        }
        if (turnNumber > 1)
        {
            RoundNext.transform.SetSiblingIndex(WarriorsList.Count - (turnNumber - 1));
            Round.text = (roundNumber + 1).ToString();//��������� ������� � ������� ������ 
        }
        else

            RoundNext.transform.SetSiblingIndex(0);
      

        Globalturn++;//
        Refreshlabels();// ��������� ������ �� ui ���������

        if (BlueLeft == 0)// �������� ������� ����������
        {
            CloseRaycast.SetActive(true);
            Whowins.text = "������� ��������";
            CancelInvoke();
        }
        else if (RedLeft == 0)
        {
            CloseRaycast.SetActive(true);
            Whowins.text = "C���� ��������";
            CancelInvoke();
            }

    }
    void Refreshlabels()//������� ��� ���������� ������ � ��������� UI
    {
        Globalturntext.text = "����� ����� ���� " + Globalturn.ToString();
        roundNumtxt.text = "����� ������ - " + roundNumber.ToString();
        turnNumtxt.text = "����� ���� � ������- " + turnNumber.ToString() + "\n" + "����� - " + WarriorsList[0].color + WarriorsList[0].number.ToString();
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
    void ActionAuto()//������� ���������� ���� ����, ���� ���� ���������� ���
    {
        if (WarriorsList[0].color != WarriorsList[1].color)
        {
            KillNext();
        }
        else
            Turn();
    }    
}
