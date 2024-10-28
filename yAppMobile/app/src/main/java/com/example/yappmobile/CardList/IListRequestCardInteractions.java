package com.example.yappmobile.CardList;

public interface IListRequestCardInteractions extends IListCardItemInteractions
{
    public void onAcceptClick(int position);
    public void onDeclineClick(int position);
}
