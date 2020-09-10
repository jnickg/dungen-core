import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-random-dungeon',
  templateUrl: './random-dungeon.component.html',
})
export class RandomDungeonComponent {
  private http: HttpClient;
  public randomDungeonBase64: string;
  public randomDungeonAlgorithm: string;
  public randomDungeonAlt: string;
  public randId: number;
  public clickedEver: boolean = false;

  constructor(http: HttpClient) {
    this.http = http;
  }

  getNewDungeon() {
    this.clickedEver = true;
    this.randId = Math.floor(Math.random() * 99998) + 1;
    var randoDungeonUrl = 'api/randomdungeon/' + this.randId;
    console.log("Nabbing " + randoDungeonUrl);
    this.http.get<RandomDungeon>(randoDungeonUrl)
      .subscribe(result => this.updateDungeon(result),
        error => console.log("Un-implemented algorithm was randomly selected, or something else went wrong."));
  }

  updateDungeon(newImage: RandomDungeon) {
    this.randomDungeonAlgorithm = newImage.algorithm;
    this.randomDungeonAlt = newImage.alt;
    this.randomDungeonBase64 = 'data:image/png;base64,' + newImage.imageBytes;
  }
}

interface RandomDungeon {
  alt: string;
  algorithm: string;
  imageBytes: ArrayBuffer;
}
