import { Injectable } from '@angular/core';
import * as CryptoJS from 'crypto-js';

@Injectable({
  providedIn: 'root'
})
export class EncryptionService {
  private readonly sharedKey = 'MoMangaMoatasem';

  constructor() {}

  async encrypt(text: string): Promise<string> {
    if (!text) return text;
    
    // Ensure key and IV are exactly 16 bytes (128 bits) by padding with zeros
    const paddedKey = this.sharedKey.padEnd(16, '\0');
    const key = CryptoJS.enc.Utf8.parse(paddedKey);
    const iv = CryptoJS.enc.Utf8.parse(paddedKey);
    
    const encrypted = CryptoJS.AES.encrypt(text, key, {
      iv: iv,
      mode: CryptoJS.mode.CBC,
      padding: CryptoJS.pad.Pkcs7
    });
    
    return encrypted.toString();
  }
}
