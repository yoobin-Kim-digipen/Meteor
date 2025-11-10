using System.Collections.Generic;

public class Colleagues
{

    public List<ColleagueData> colleagueList = new List<ColleagueData>()
    {
        new ColleagueData()
        {
            colleague_id = 1,
            image_path = "Colleague_Image/Gambler",
            name = "Hans",
            Class = "Gambler",
            down_pay = 1000,
            cost = 100
        },
        new ColleagueData()
        {
            colleague_id = 1,
            image_path = "Colleague_Image/Gambler",
            name = "Kal",
            Class = "Gambler",
            down_pay = 1000,
            cost = 100
        },
        new ColleagueData()
        {
            colleague_id = 1,
            image_path = "Colleague_Image/Gambler",
            name = "Ed",
            Class = "Gambler",
            down_pay = 1000,
            cost = 100
        },
        new ColleagueData()
        {
            colleague_id = 1,
            image_path = "Colleague_Image/Gambler",
            name = "Book",
            Class = "Gambler",
            down_pay = 1000,
            cost = 100
        },
    };

    ~Colleagues()
    {
        colleagueList.Clear();

    }
}
