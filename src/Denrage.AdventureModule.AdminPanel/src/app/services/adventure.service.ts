import { Injectable } from '@angular/core';
import { Adventure } from '../models/adventure';
import { UserService } from './user.service';

@Injectable({
  providedIn: 'root'
})
export class AdventureService {

  constructor(private userService: UserService) { }

  public getAdventures(): Adventure[] {
    return [
      { name: "HelloWorld1", steps: [ { name: "step1" }, { name: "step2" }, { name: "step3" } ], picture: "https://picsum.photos/200/100", groups: [
        { name: "Group1", color: "#32CD32", step: "step1", users: [
          {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: "", ingameUser: { accountName: "honky.3864", characterName: "Athena Tholaen", positionX: 48000, positionY: 30720 }},
          {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: "", ingameUser: { accountName: "Denrage.3864", characterName: "Seria", positionX: 50000, positionY: 30720 }},
          {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: "", ingameUser: { accountName: "whitetiger.7425", characterName: "Gwendolyn", positionX: 48000, positionY: 20720 }},
          {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: "", ingameUser: { accountName: "xTaddelx.SomeNumber", characterName: "KeineAhnung", positionX: 18000, positionY: 30720 }},
        ] },
        { name: "Group2", step:"step2", color: "#B22222", users: [
          {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: "", ingameUser: { accountName: "honky.3865", characterName: "Athena Tholaen", positionX: 38000, positionY: 30720 }},
          {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: "", ingameUser: { accountName: "Denrage.3865", characterName: "Seria", positionX: 60000, positionY: 30720 }},
          {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: "", ingameUser: { accountName: "whitetiger.7435", characterName: "Gwendolyn", positionX: 48000, positionY: 24720 }},
          {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: "", ingameUser: { accountName: "xTaddelx.Someumber", characterName: "KeineAhnung", positionX: 39000, positionY: 30720 }},
        ] }
      ] },
      { name: "HelloWorld2", steps: [ { name: "step1" }, { name: "step2" }, { name: "step3" } ], picture: "https://picsum.photos/200/100", groups: [
        { name: "This is an awesome group", step:"step1", color: "red", users: [] }
      ] },
      { name: "HelloWorld3", steps: [ { name: "step1" }, { name: "step2" }, { name: "step3" } ], picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld4", steps: [ { name: "step1" }, { name: "step2" }, { name: "step3" } ], picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld5", steps: [ { name: "step1" }, { name: "step2" }, { name: "step3" } ], picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld6", steps: [ { name: "step1" }, { name: "step2" }, { name: "step3" } ], picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld7", steps: [ { name: "step1" }, { name: "step2" }, { name: "step3" } ], picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld8", steps: [ { name: "step1" }, { name: "step2" }, { name: "step3" } ], picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld9", steps: [ { name: "step1" }, { name: "step2" }, { name: "step3" } ], picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld10", steps: [ { name: "step1" }, { name: "step2" }, { name: "step3" } ], picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld11", steps: [ { name: "step1" }, { name: "step2" }, { name: "step3" } ], picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld12", steps: [ { name: "step1" }, { name: "step2" }, { name: "step3" } ], picture: "https://picsum.photos/200/100", groups: [] },
    ]
  }
}
