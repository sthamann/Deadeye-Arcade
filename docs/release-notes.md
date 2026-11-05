Keep an arcade loader’s console from interrupting its active renderer and retain combined Shoot / Start actions in input replay.

- Restore game focus from a classic console only when the console and the visible FREEGLUT renderer belong to the exact same process in an active TeknoParrot session. Keep the console available for diagnosis, preserve other applications and dialogs, and record verified recoveries.
- Keep Alien3’s combined Shoot / Start action in the MAME gameplay replay. Standalone Coin and Start rows stay in the startup sequence; a combined fire action is no longer skipped because its label contains Start.

Native tests still require actual stable game focus. Received HID packets and readable mappings remain separate from visual gameplay and physical lightgun acceptance.
