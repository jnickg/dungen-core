import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RandomDungeon } from './RandomDungeon';

@Component({
  selector: 'app-random-dungeon',
  templateUrl: './random-dungeon.component.html',
})
export class RandomDungeonComponent {
  public randomDungeonBase64: string;
  public randomDungeonGifBase64: string;
  public randomDungeonAlgorithm: string;
  public randomDungeonAlt: string;
  public randId: number;
  public clickedEver = false;

  constructor(private http: HttpClient) {
  }

  getNewDungeon() {
    this.clickedEver = true;
    this.randId = Math.floor(Math.random() * 99998) + 1;
    let randoDungeonUrl = 'api/randomdungeon/' + this.randId;
    console.log('Nabbing ' + randoDungeonUrl);
    this.http.get<RandomDungeon>(randoDungeonUrl)
      .subscribe(result => this.updateDungeon(result),
        err => console.log('Un-implemented algorithm was randomly selected, or something else went wrong. (Error: ' + err.error + ')'));
  }

  updateDungeon(newImage: RandomDungeon) {
    this.randomDungeonAlgorithm = newImage.algorithm;
    this.randomDungeonAlt = newImage.alt;
    this.randomDungeonBase64 = 'data:image/png;base64,' + newImage.imageBytes;
    this.randomDungeonGifBase64 = 'data:image/gif;base64,' + newImage.gifBytes;
  }
}

