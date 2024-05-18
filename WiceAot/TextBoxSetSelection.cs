namespace Wice;

public enum TextBoxSetSelection
{
    Left,               // cluster left
    Right,              // cluster right
    Up,                 // line up
    Down,               // line down
    PageUp,             // page up
    PageDown,           // page down
    LeftChar,           // single character left (backspace uses it)
    RightChar,          // single character right
    LeftWord,           // single word left
    RightWord,          // single word right
    Home,               // front of line
    End,                // back of line
    First,              // very first position
    Last,               // very last position
    AbsoluteLeading,    // explicit position (for mouse click)
    AbsoluteTrailing,   // explicit position, trailing edge
    All                 // select all text
}
