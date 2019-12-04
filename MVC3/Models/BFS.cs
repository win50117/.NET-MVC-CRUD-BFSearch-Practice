using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Collections;
using System.Web.Caching;
using System.Web.Mvc;

namespace MVC3.Models
{
    public class BFS
    {
        private bool[] visit = new bool[20];
        public Product BFSIn(List<Product> _PDList, string _SearchTitle)
        {
            Queue myQueue = new Queue();

            if (_PDList[0].Title == _SearchTitle) //如果搜尋項目等於起始節點項目
            {             
                return _PDList[0];
            }
            else
            {
                myQueue.Enqueue(_PDList[0].Title);//放入根節點
                visit[0] = true; //根節點標記已探索
            }
            
            while (myQueue.Count != 0)       //若queue不是空的 
            {
                string Head = myQueue.Peek().ToString(); //存取queue頭節點  
                myQueue.Dequeue();//將節點取出                

                for (int i = 0; i < _PDList.Count; i++)
                {
                    if (visit[i] == false)      //若鄰節點未被探索過 
                    {
                        if (_PDList[i].Tail == Head)
                        {
                            if (_PDList[i].Title == _SearchTitle) //如果搜尋項目等於臨節點
                            {
                                return _PDList[i];
                            }
                            myQueue.Enqueue(_PDList[i].Title); //將臨節點放入
                            visit[i] = true;
                        }
                    }                    
                }
            }
            return (null);
        }
    }
}