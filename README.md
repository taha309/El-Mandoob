# المندوب — El Mandoob

**El Mandoob** is an Egyptian Arabic delivery arcade/story game built in Unity.

The project is being developed from the open-source **Delivery Boy** project by the 1xBest student team. The original project is MIT licensed; attribution is preserved below and in the repository license history.

## Current Direction

The player works as a delivery rider in a compact Cairo/Giza-inspired district. Normal jobs introduce local businesses and recurring customers while special deliveries gradually form a neighborhood mystery.

The game is designed around a focused loop rather than a giant open world:

1. Receive a delivery job.
2. Reach the pickup business.
3. Collect the order.
4. Navigate traffic and obstacles.
5. Deliver to the customer.
6. Earn EGP, tips and reputation.
7. Upgrade speed/endurance.
8. Unlock harder shifts and story deliveries.

## Language

Egyptian Arabic (`ar-EG`) is the original/default language for the game.

- Egyptian Arabic UI and dialogue
- Right-to-left Arabic shaping through RTLTMPro
- Egyptian names, businesses, neighborhoods and customer messages
- English can be added later as an optional localization

## Story

The first neighborhood arc starts as an ordinary delivery job.

The player becomes familiar with recurring customers such as an elderly customer who orders medicine and local business owners who recognize the rider. Later, a customer named Shereef starts sending unusually specific sealed-document deliveries. A recipient named Nada says she never requested one of the envelopes, addresses begin changing at the last moment, and a local restaurant owner warns the player that the sender seems more interested in couriers than his deliveries.

The first arc concludes when the player receives documents tying several suspicious orders and refund records together. The game remains non-combat: tension comes from routes, messages, customers and information discovered through work.

## Progression

El Mandoob currently tracks:

- EGP balance
- delivery count
- completed shifts
- reputation
- tips
- story chapter
- speed upgrades
- endurance upgrades
- unlocked shifts
- best star rating for each shift

Progress is stored in `el_mandoob_save.json` under Unity's persistent data path.

## Controls

### Mobile
The original on-screen joystick remains supported.

### Desktop
- `WASD` or arrow keys: move
- UI buttons: receive/deliver/pause/menu actions

## Unity Version

Use **Unity 2021.3.24f1** for this project.

Do not upgrade the project merely because a newer Unity version is installed.

## Main Scenes

- `Assets/Scenes/menu.unity`
- `Assets/Scenes/Scene_Base.unity`
- `Assets/Scenes/NewScene.unity`

## Development Branch

Active conversion work is being developed on:

`dev/el-mandoob`

## Third-Party Assets

The upstream project contains imported art, fonts, packages and other third-party material. The MIT license for the project code does not automatically prove that every imported asset is covered by the same terms.

See `THIRD_PARTY_ASSET_AUDIT.md` before any public/commercial release.

## Upstream Project Attribution

El Mandoob is based on **Delivery Boy**:

https://github.com/phamson02/DeliveryBoy-UnityGame

Delivery Boy was developed for the Introduction to Software Engineering course at Hanoi University of Science and Technology by the 1xBest team:

- Hồ Minh Khôi — https://github.com/hmkhoi2701
- Nguyễn Ngọc Toàn — https://github.com/nntoan209
- Trương Quang Bình — https://github.com/quangbinh113
- Trần Cát Khánh — https://github.com/khanhha1005
- Nguyễn Thiên Hoàn — https://github.com/Bigbynth
- Phạm Tiến Sơn — https://github.com/phamson02

## License

The upstream Delivery Boy project is distributed under the MIT License. Keep the existing `LICENSE` file and required copyright/license notices when distributing derivative code.

Asset licenses must be audited separately before release.
