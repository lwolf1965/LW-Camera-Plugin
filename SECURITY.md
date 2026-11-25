# Security Policy

## Supported versions

Only the latest version is supported.

## What the plugin does and does not do

- It runs inside the game as a Pulsar plugin, compiled from this source on your machine.
- It is client side only: it changes the view, nothing in the world.
- It makes **no network connections**.
- It reads and writes only its own settings file and, when you ask for it, the weapon report (`weapons.txt`
  in the plugin's data folder). Its messages go to the game log.

A change that breaks any of these points is a security problem. Please report it.

## Reporting a vulnerability

Please do **not** open a public issue for a security problem.

Use GitHub's private reporting: the **Security** tab of this repository, then **Report a vulnerability**.
Include the plugin version, what you found, and how to reproduce it.

This is a hobby project with one maintainer. Expect a first answer within about a week.
