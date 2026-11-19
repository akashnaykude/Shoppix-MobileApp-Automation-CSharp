@forgotpassword
Feature: Forgot password screen

  # ---------- UI content checks ----------
  Scenario: Forgot password screen loads with expected UI content
    Given user is on forgot password screen
    Then forgot password screen should display expected UI content

  Scenario: Forgot password screen shows heading
    Given user is on forgot password screen
    Then forgot password heading should be visible

  Scenario: Forgot password screen shows description text
    Given user is on forgot password screen
    Then forgot password description text should be visible

  Scenario: Forgot password screen shows email field label
    Given user is on forgot password screen
    Then forgot password email field label should be visible

  Scenario: Forgot password screen shows email input placeholder
    Given user is on forgot password screen
    Then forgot password email input should show placeholder text

  Scenario: Forgot password screen shows Send button
    Given user is on forgot password screen
    Then forgot password Send button should be visible

  Scenario: Forgot password screen shows back button
    Given user is on forgot password screen
    Then forgot password back button should be visible

  # ---------- Functional / validation ----------
  Scenario: Reset fails with invalid email format
    Given user is on forgot password screen
    When user enters an invalid email for reset
    Then user cant tap forgot password Send button

  Scenario: Unable to Reset when email field is empty
    Given user is on forgot password screen
    When user leaves reset email field empty
    Then user cant tap forgot password Send button

  Scenario: Reset with unregistered email is handled gracefully
    Given user is on forgot password screen
    When user enters an unregistered email for reset
    And user taps forgot password Send button
    Then unregistered reset email behavior should be handled

  Scenario: Email field accepts a valid email on reset screen
    Given user is on forgot password screen
    When user enters a valid registered email for reset
    Then forgot password email input should show entered value
    And Send button should get enabled

  Scenario: User can reset password via emailed temporary password
    Given user is on forgot password screen
    When user enters a registered email for reset
    And user taps forgot password Send button
    Then reset link confirmation message should be displayed
    When user retrieves the temporary password from email
    And user logs in with the temporary password
    Then user should be logged in successfully

  # ---------- Navigation ----------
  Scenario: User can go back from forgot password screen to login
    Given user is on forgot password screen
    When user taps forgot password back button
    Then login screen should be displayed

  Scenario: Android back from forgot password returns to login
    Given user is on forgot password screen
    When user presses Android back
    Then login screen should be displayed
