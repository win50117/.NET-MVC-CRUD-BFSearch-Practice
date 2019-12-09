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
        public Product BFSIn(List<Product> _PDList, string _SearchTitle)//使用資料List和搜尋關鍵字參數
        {
            bool[] Visit = new bool[_PDList.Count];//宣告和資料數量相等的陣列，用於存取節點是否被探索
            Queue myQueue = new Queue();

            if (_PDList[0].Title == _SearchTitle) //如果搜尋項目等於起始節點，回傳節點資料
            {             
                return _PDList[0];
            }
            else
            {
                myQueue.Enqueue(_PDList[0].Title);//放入根節點
                Visit[0] = true; //根節點標記已探索
            }
            
            while (myQueue.Count != 0)      //若queue不是空的 
            {
                string Head = myQueue.Dequeue().ToString(); //存取queue頭節點並取出

                for (int i = 0; i < _PDList.Count; i++)
                {
                    if (Visit[i] == false)  //若鄰節點未被探索過 
                    {
                        if (_PDList[i].ParentNode == Head)
                        {
                            if (_PDList[i].Title == _SearchTitle) //搜尋項目等於臨節點，回傳目標節點
                            {
                                return _PDList[i];
                            }
                            myQueue.Enqueue(_PDList[i].Title); //將臨節點放入
                            Visit[i] = true;
                        }
                    }                    
                }
            }
            return (null);
        }
    }
}