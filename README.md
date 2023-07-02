## Gameplay
- **General**
  - **Low Health Threshold**
    - ~~At or below 25%~~ ⇒ Below 25% </br> *Developer Notes: Have fun using Blood Shrines a second time. Just...be careful.*
- **Survivors**
  - **Commando**
    - Double Tap
      - Slightly increased the duration of the second shot
    - Tactical Dive
      - No longer cancels sprinting for a frame on activation </br> *Developer Notes: Missing the sprint bonus caused dive to travel less distance than expected. It should be a much more viable option now.*
    - Suppressive Fire
      - Base Bullet Count: ~~6~~ ⇒ 8
      - Now has a much more prominent 'auto-aim' effect
    - Frag Grenade
      - Explosion Damage: ~~700%~~ ⇒ 900%
      - Blast Damage Falloff: ~~Sweetspot~~ ⇒ Linear
      - 1s detonation fuse begins immediately rather than after impact </br> *Developer Notes: Both of Commando's special skills were frequently underwhelming. Suppressive Fire should feel more distinct from Double Tap now; Frag Grenades will be doing roughly the same damage but offer a better reward for perfect explosion placement.*
  - **Huntress**
    - Arrow Rain
      - Proc Coefficient Per Tick: ~~0.2~~ ⇒ 0.5
      - Slightly extended hitbox upwards to better match visuals
      - Update description to include accurate damage per second values
  - **Bandit**
    - Blast
      - Reduced spread bloom per shot
    - Serrated Dagger
      - Lunges Bandit forwards a short distance when swinging (as the description already indicated)
    - Serrated Shiv
      - Damage: ~~240%~~ ⇒ 140%
      - Base Stock: ~~1~~ ⇒ 2
      - Improve consistency of the projectile's trajectory against close targets
    - Smoke Bomb
      - Cooldown: ~~6s~~ ⇒ 8s </br> *Developer Notes: Because Smoke Bomb's cooldown ticks down even while the smoke is active, the skill had an incredibly short effective cooldown. A slight cooldown increase should result in more thoughtful Smoke Bomb use and give Lights Out a chance to shine.*
  - **Engineer**
    - Bouncing Grenades
      - Gains 1 grenade charge immediately upon activating the skill. </br> *Developer Notes: Bouncing Grenades had an uncomfortable period of zero feedback until the first grenade was charged. This change won't be a massive buff, but it should make the skill feel less awkward to use.*
    - Pressure Mines
      - Unarmed Mine Explosion Radius: ~~1.6m~~ ⇒ 3.2m
    - Spider Mines
      - Explosion Damage: ~~600%~~ ⇒ 200%
      - Explosion Force: ~~1000~~ ⇒ 400
      - Explosion Radius: ~~14m~~ ⇒ 8m
      - Each mine now explodes up to **3 times** before expiring
    - Bubble Shield Radius: ~~10m~~ ⇒ 11m </br> *Developer Notes: This is a very minor buff to Bubble Shield, but the increased radius should help it interact better with the camera and feel less claustrophobic.*
    - TR58 Carbonizer Turret
      - Walking turrets now sprint when chasing enemies
  - **Artificer**
    - Flamethrower Total Damage: ~~2000%~~ ⇒ 2400% </br> *Developer Notes: Burn changes in SotV massively reduced the damage potential of Flame Bolt and Flamethrower. Flame Bolt recieved a damage increase to compensate, but Flamethrower didn't...until now!*
  - **Mercenary**
    - Horizontal distance travelled by Whirlwind and Rising Thunder is no longer reduced by attack speed
    - Eviscerate no longer attempts to target allies
  - **REX**
    - Bramble Volley
      - Healing per hit: ~~10%~~ ⇒ 5%
    - DIRECTIVE: Harvest
      - Skill Rework: Deals 25% max hp as damage every 3 seconds to grow a healing fruit. Fruits drop on death or after a maximum of 8 is reached.
      - Fruit Duration: ~~20s~~ ⇒ 30s
      - Fruits no longer gravitate towards allies already at full health
      - Projectile Speed: ~~120m/s~~ ⇒ 130m/s (parity with DIRECTIVE: Inject)
      - Fixed improper projectile collider and network settings that led to unreliable collisions </br> *Developer Notes: DIRECTIVE: Harvest was a great idea on paper that was very clunky in practice. The additional damage opens up new build possibilities, and it should feel like less of a dead slot when battling final bosses now, too.*
    - DIRECTIVE: Harvest AND Tangling Growth
      - Duration: ~~1s~~ ⇒ 0.8s
      - End lag can be cancelled by both of REX's utilities
  - **Acrid**
    - Blight now reduces the victims armor by 5 per stack </br> *Developer Notes: Getting large stacks of Blight on bosses and the like is fun, but the passive really just serves as a damage increase for your poisonous skills. Now, Blight stacks can more actively interact with other Blight stacks and Acrid's other skills.*
    - Vicious Wounds now resets to the first swing of the combo when cancelled by sprint </br> *Developer Notes: Systems changes in previous updates caused Acrid to be able to sprint cancel through the entire Vicious Wounds combo. This means there is no reason not to use the sprint cancel, so that change has been reverted.*
    - Frenzied Leap Damage: ~~550%~~ ⇒ 600%
  - **Captain**
    - Vulcan Shotgun bullets have no damage falloff when fully charged
    - Beacon: Hacking
      - Reduces costs over time while hacking
      - Increase hacking time by +50%
  - **Railgunner**
    - HH44 Marksman
      - Damage Per Shot: ~~400%~~ ⇒ 500%
      - Base Stock: ~~Infinite~~ ⇒ 10
      - Passively reloads stock while not firing </br> *Developer Notes: HH44 Marksman having no interactions with items like Backup Magazine didn't sit right.*
  - **Void Fiend**
    - Passive Healing to Corruption Ratio: ~~1:1~~ ⇒ 2:1
    - Suppress
      - Base Stock: ~~Infinite~~ ⇒ 2
    - Suppress AND Corrupted Suppress
      - Cast Duration: ~~2s~~ ⇒ 1.6s
- **Items**
  - **Bison Steak**
    - Grants +0.2 hp/s regeneration per stack in addition to maximum health
  - **Delicate Watch**
    - Damage Boost: ~~20% (+20% per stack)~~ ⇒ 12% (+12% per stack)
    - Broken watches are now reset one at a time, on the minute </br> *Developer Notes: Most players dislike permanently losing items. Breaking one or two watches will be less punishing than before, but large watch stacks will still be a large gamble.*
  - **Medkit**
    - Flat Healing: ~~20hp~~ ⇒ 10hp
  - **Monster Tooth**
    - Flat Healing: ~~8hp~~ ⇒ 4hp
  - **Oddly-shaped Opal**
    - Armor Bonus: ~~100 (+100 per stack)~~ ⇒ 60 (+60 per stack)

## Quality of Life
- Fixed some materials not dithering properly, notably on Void Fiend
- Added descriptions for Void Fiend's corrupted skillset when in a run
- Update Bandit's Serrated Shiv visuals to be more consistent between throws
- Update Commando's Frag Grenade explosion SFX and VFX
- Update Railgunner's Polar Field Device VFX and SFX
- Fix Void Fiend's corruption meter flickering when permanently corrupted
