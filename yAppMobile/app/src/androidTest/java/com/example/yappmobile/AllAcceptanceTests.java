package com.example.yappmobile;

import com.example.yappmobile.tests.*;

import org.junit.runner.RunWith;
import org.junit.runners.Suite;

@RunWith(Suite.class)
@Suite.SuiteClasses({
        PostingAT.class,
        ProfileManagementAT.class
})
public class AllAcceptanceTests {
}
