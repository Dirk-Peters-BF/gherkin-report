Feature: A Stack

    Scenario: Empty Stack Size
        Given an empty stack
        Then the current stack size is 0

    Scenario: Pop Pushed Item
        Given an empty stack
        When 1 is pushed
        And one item is popped
        Then the current stack size is 0