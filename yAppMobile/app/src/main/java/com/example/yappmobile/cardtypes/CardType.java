package com.example.yappmobile.cardtypes;

public enum CardType
{
    CURRENT_FRIEND(1),
    FRIEND_REQUEST(2),
    POST(3);

    private final int value;
    private CardType(int value)
    {
        this.value = value;
    }

    public int getValue()
    {
        return value;
    }
};