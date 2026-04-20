import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface LuckyPrize {
  id?: number;
  title: string;
  titleAr: string;
  description: string;
  descriptionAr: string;
  icon: string;
  color: string;
  probabilityWeight: number;
  isActive: boolean;
}

export interface LuckyRewardsDrawResult {
  prizeId: number;
  title: string;
  titleAr: string;
  description: string;
  descriptionAr: string;
  icon: string;
  color: string;
  remainingCoins: number;
}

@Injectable({
  providedIn: 'root'
})
export class LuckyRewardsService {
  private apiUrl = environment.apiUrl + '/luckyrewards';

  constructor(private http: HttpClient) {}

  getStatus(): Observable<{isEnabled: boolean}> {
    return this.http.get<{isEnabled: boolean}>(`${this.apiUrl}/status`);
  }

  getUserCoins(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/coins`);
  }

  drawReward(): Observable<LuckyRewardsDrawResult> {
    return this.http.post<LuckyRewardsDrawResult>(`${this.apiUrl}/draw`, {});
  }

  // --- ADMIN METHODS ---
  getPrizes(): Observable<LuckyPrize[]> {
    return this.http.get<LuckyPrize[]>(`${this.apiUrl}/prizes`);
  }

  addPrize(prize: LuckyPrize): Observable<LuckyPrize> {
    return this.http.post<LuckyPrize>(`${this.apiUrl}/prizes`, prize);
  }

  updatePrize(prize: LuckyPrize): Observable<LuckyPrize> {
    return this.http.put<LuckyPrize>(`${this.apiUrl}/prizes`, prize);
  }

  deletePrize(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/prizes/${id}`);
  }
}
