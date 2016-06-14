using System;

namespace HypercardHome
{
    public class BackgroundButton68
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Welcome to...");
        }
    }

    public class BackgroundButton3
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Stack Kit");
        }
    }

    public class BackgroundButton2
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Card 3");
        }
    }

    public class BackgroundButton107
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Card 4");
        }
    }

    public class BackgroundButton149
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Card 5");
        }
    }

    public class BackgroundButton169
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.PreviousCard();
        }
    }

    public class BackgroundButton59
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.PreviousCard();
        }
    }

    public class BackgroundButton56
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.PreviousCard();
        }
    }

    public class BackgroundButton170
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.NextCard();
        }
    }

    public class BackgroundButton58
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.NextCard();
        }
    }

    public class BackgroundButton55
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.NextCard();
        }
    }

    public class Background2717
    {
        public static void OpenCard(HyperCard.Background background)
        {
        }
    }

    public class Card5698
    {
        public static void UpdateUserLevel(HyperCard.Stack stack, HyperCard.Card card, int userLevel)
        {
            if (stack.UserLevel == (HyperCard.UserLevel)userLevel) return;

            // set the stack user level
            stack.UserLevel = (HyperCard.UserLevel)userLevel;

            // move the arrow button to the correct location
            card.GetPartFromName("Arrow").Rect.Top = card.GetPartFromName("UserLevel " + userLevel).Rect.Top;

            // set the highlight of the buttons correctly
            for (int i = 1; i < 6; i++)
                card.GetPartFromName("UserLevel " + i).Highlight = (i <= userLevel);

            card.GetPartFromID(1).Visible = userLevel >= 5;
            card.GetPartFromID(3).Visible = userLevel >= 3;
            card.GetPartFromID(2).Visible = userLevel >= 2;

            stack.Renderer.Invalidate();
        }
    }

    public class CardButton8
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            Card5698.UpdateUserLevel(stack, (HyperCard.Card)button.Parent, 5);
        }
    }

    public class CardButton7
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            Card5698.UpdateUserLevel(stack, (HyperCard.Card)button.Parent, 4);
        }
    }

    public class CardButton6
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            Card5698.UpdateUserLevel(stack, (HyperCard.Card)button.Parent, 3);
        }
    }

    public class CardButton5
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            Card5698.UpdateUserLevel(stack, (HyperCard.Card)button.Parent, 2);
        }
    }

    public class CardButton4
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            Card5698.UpdateUserLevel(stack, (HyperCard.Card)button.Parent, 1);
        }
    }
}
