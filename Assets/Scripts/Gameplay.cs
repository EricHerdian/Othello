using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Gameplay : MonoBehaviour
{
    public Text winnerTitle;    // ui text
    public GameObject Panel;    // ui panel
    int state = 0;  // game state
    int turn;   // turn : {1 => white; 2 => black}
    GameObject[,] board = new GameObject[8,8];  // board
    int[,] field = new int[8,8];    // board value
    public GameObject disc; // dics
    public AudioSource src;    // audio source
    public AudioClip discFlip;  // disc flip
    public AudioClip winAudio;  // win
    Sprite[] discs; // disc sprites

    private void Start()
    {
        discs = Resources.LoadAll<Sprite>("Sprites/Board");
        src.clip = discFlip;
        
        for(int i=0; i<8; i++){
            for(int j=0; j<8; j++){
                board[i,j] = Instantiate(disc, new Vector3((i-4)*32+16, (j-4)*32+16, 0), Quaternion.identity);
                field[i,j] = 0;
            }
        }

        // Starting Board Setup
        Sprite sr = null;
        sr = System.Array.Find<Sprite>(discs, (Sprites) => Sprites.name.Equals("Board_2"));
        board[3,3].GetComponent<SpriteRenderer>().sprite = sr;
        board[4,4].GetComponent<SpriteRenderer>().sprite = sr;
        field[3,3] = 1;
        field[4,4] = 1;
        sr = System.Array.Find<Sprite>(discs, (Sprites) => Sprites.name.Equals("Board_3"));
        board[3,4].GetComponent<SpriteRenderer>().sprite = sr;
        board[4,3].GetComponent<SpriteRenderer>().sprite = sr;
        field[3,4] = 2;
        field[4,3] = 2;

        state = 1;
        turn = 1;
    }

    bool isPut(int x, int y)
    {
        // This function is to know where is playable disc location in red dot each of player turn
        // coordinate around disc
        int[] px = new int[]{-1, 0, 1, -1, 1, -1, 0, 1};
        int[] py = new int[]{1, 1, 1, 0, 0, -1, -1, -1};
        bool cPut = false;

        for(int ind=0; ind<8; ind++){
            if(x+px[ind] >= 0 && x+px[ind] < 8 && y+py[ind] >= 0 && y+py[ind] < 8)
            {
                if(
                    field[x,y] == 0 &&
                    turn != field[x+px[ind], y+py[ind]] &&
                    field[x+px[ind], y+py[ind]] != 0
                )
                {
                    int nx = px[ind] + x;
                    int ny = py[ind] + y;
                    while(true)
                    {
                        nx += px[ind];
                        ny += py[ind];
                        if(nx < 0 || nx >= 8 || ny < 0 || ny >= 8 || field[nx, ny] == 0) break;
                        if(field[nx,ny] != 0 && field[nx,ny] == turn)
                        {
                            cPut = true;
                            break;
                        }
                    }
                }
            }
        }

        return cPut;
    }

    void refreshField(int x, int y)
    {
        // This function is to refresh field point
        // coordinate around disc
        int[] px = new int[]{-1, 0, 1, -1, 1, -1, 0, 1};
        int[] py = new int[]{1, 1, 1, 0, 0, -1, -1, -1};

        for(int ind=0; ind<8; ind++){
            if(x+px[ind] >= 0 && x+px[ind] < 8 && y+py[ind] >= 0 && y+py[ind] < 8)
            {
                if(
                    field[x,y] == 0 &&
                    turn != field[x+px[ind], y+py[ind]] &&
                    field[x+px[ind], y+py[ind]] != 0
                )
                {
                    int nx = px[ind] + x;
                    int ny = py[ind] + y;
                    while(true)
                    {
                        nx += px[ind];
                        ny += py[ind];
                        if(nx < 0 || nx >= 8 || ny < 0 || ny >= 8 || field[nx, ny] == 0) break;
                        if(field[nx, ny] != 0 && field[nx, ny] == turn)
                        {
                            nx -= px[ind];
                            ny -= py[ind];
                            while(field[nx, ny] != turn && field[nx, ny] != 0){
                                field[nx, ny] = turn;
                                nx -= px[ind];
                                ny -= py[ind];
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    private void Update()
    {
        // Refresh board according to field value
        if(state == 1){
            bool canPut = false;
            for(int i=0; i<8; i++){
                for(int j=0; j<8; j++){
                    Sprite sr = null;
                    if(isPut(i, j))
                    {
                        canPut = true;
                        sr = System.Array.Find<Sprite>(discs, (Sprites) => Sprites.name.Equals("Board_1"));
                        board[i,j].GetComponent<SpriteRenderer>().sprite = sr;
                    }
                    else if(field[i,j] == 0)
                    {
                        sr = System.Array.Find<Sprite>(discs, (Sprites) => Sprites.name.Equals("Board_0"));
                        board[i,j].GetComponent<SpriteRenderer>().sprite = sr;
                    }
                    else if(field[i,j] == 1)
                    {
                        sr = System.Array.Find<Sprite>(discs, (Sprites) => Sprites.name.Equals("Board_2"));
                        board[i,j].GetComponent<SpriteRenderer>().sprite = sr;
                    }
                    else if(field[i,j] == 2){
                        sr = System.Array.Find<Sprite>(discs, (Sprites) => Sprites.name.Equals("Board_3"));
                        board[i,j].GetComponent<SpriteRenderer>().sprite = sr;
                    }
                }
            }
            if(!canPut)
            {
                if(turn == 1) turn = 2;
                else turn = 1;

                for(int i=0; i<8; i++){
                    for(int j=0; j<8; j++){
                        if(isPut(i, j))
                        {
                            canPut = true;
                        }
                    }
                }

                if(!canPut)
                {
                    state = 3;
                }
            }
            else
            {
                state = 2;
            }
        }

        // Player Turn
        if(state == 2){
            if(Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Input.mousePosition;
                Debug.Log(mousePosition.x + " " + mousePosition.y);
                int input_X = (int)mousePosition.x / 64;
                int input_Y = (int)mousePosition.y / 64;
                Debug.Log(input_X + " " + input_Y);
                
                if(isPut(input_X, input_Y))
                {
                    src.Play();
                    refreshField(input_X, input_Y);
                    if(turn == 1)
                    {
                        field[input_X, input_Y] = 1;
                        turn = 2;
                    } 
                    else if (turn == 2)
                    {
                        field[input_X, input_Y] = 2;
                        turn = 1;
                    }
                    state = 1;
                }
            }
        }

        if(state == 3)
        {
            int w = 0;
            int b = 0;
            for(int i=0; i<8; i++){
                for(int j=0; j<8; j++){
                    if(field[i, j] == 1) w++;
                    else if(field[i, j] == 2) b++;
                }
            }
            Debug.Log("black: "+ b +" white: " + w);

            if(b > w) winnerTitle.text = "Black Win!";
            else if(b < w) winnerTitle.text = "White Win!";
            else winnerTitle.text = "Draw";
            Panel.SetActive(true);

            src.clip = winAudio;
            src.Play();

            state = 4;
        }
    }

    public void Retry()
    {
        SceneManager.LoadScene("Game");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

