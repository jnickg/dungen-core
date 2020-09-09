import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-random-dungeon',
  templateUrl: './random-dungeon.component.html',
})
export class RandomDungeonComponent {
  private http: HttpClient;
  public randomDungeonBase64: string;
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
    this.http.get(randoDungeonUrl, { responseType: 'blob' })
      .subscribe(result => this.updateDungeon(result),
        error => console.log("Un-implemented algorithm was randomly selected, or something else went wrong."));
  }

  updateDungeon(newImage: Blob) {
    var reader = new FileReader();
    reader.readAsDataURL(newImage);
    reader.onload = (function (component, reader) {
      return function (e) {
        var binaryData = reader.result;
        if (typeof binaryData === 'string') {
          component.randomDungeonBase64 = binaryData;
          console.log("Processed base64: " + binaryData);
        }
      };
    })(this, reader);
  }

  assignBase64(fr: FileReader) {

  }
}
