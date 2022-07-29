import { Injectable } from '@angular/core';
import { Guid } from 'guid-typescript';

@Injectable({
  providedIn: 'root'
})
export class PlayerService {

  players: Player[] = [
    { id: Guid.parse("f683dad2-9a68-49ba-b6f5-0d91964607d3"), name: "Denrage", connected: true, image: "https://cdn.discordapp.com/avatars/225594944247693323/14042fda7b551ef22107105e533322ba.webp" },
    { id: Guid.parse("bd2d7760-fcaf-4246-9324-4d96ab7d85eb"), name: "Denrage2", connected: true, image: "https://cdn.discordapp.com/avatars/225594944247693323/14042fda7b551ef22107105e533322ba.webp" },
    { id: Guid.parse("c59cfe25-cb75-4783-a024-5bd709053a16"), name: "whitetiger8D", connected: true, image: "https://cdn.discordapp.com/avatars/234286875374780418/309b1dd578cc98db91adab0543cb44c2.webp" },
    { id: Guid.parse("91c6f817-8597-4f54-a506-ec032bdf436c"), name: "Denrage3", connected: false, image: "https://cdn.discordapp.com/avatars/225594944247693323/14042fda7b551ef22107105e533322ba.webp" },
    { id: Guid.parse("f0e8a302-1d1d-45f3-9229-8dc601dd5ecb"), name: "Denrage4", connected: true, image: "https://cdn.discordapp.com/avatars/225594944247693323/14042fda7b551ef22107105e533322ba.webp" }
  ]

  constructor() { }

  getPlayer(playerId: Guid): Player {
    return this.players.filter(player => player.id.equals(playerId))[0];
  }

  addPlayer(player: Player) {
    this.players.push(player);
  }

  deletePlayer(player: Player) {
    this.players.splice(this.players.indexOf(player), 1);
  }
}

export interface Player {
  id: Guid;
  name: string;
  connected: boolean;
  image: string; // url on the asp-server or an identifier to get it from there idk
}
