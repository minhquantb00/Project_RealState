import axios from '@axios'
import { defineStore } from 'pinia'

export const useBillStore = defineStore('useBillStore', {
  actions: {
    getAllBill(params){
      return new Promise((resolve, reject) => {
        axios.put(`/phieuxemnha/GetAllPhieuXemNha?pageNumber=${params.pageNumber}&pageSize=${params.pageSize}`, { ...params })
          .then(res => resolve(res))
          .catch(error => reject(error))
      })
    },
    createBill(nhaId, nhanVienId, params){
      return new Promise((resolve, reject) => {
        axios.post(`/phieuxemnha/CreatePhieuXemNha?nhaId=${nhaId}&nhanVienId=${nhanVienId}`, { ...params }, {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        })
          .then(res => resolve(res))
          .catch(error => reject(error))
      })
    },
    updateBill(params){
      return new Promise((resolve, reject) => {
        axios.put('/phieuxemnha/UpdatePhieuXemNha', { ...params })
          .then(res => resolve(res))
          .catch(error => reject(error))
      })
    },
    deleteBill(phieuXemNhaId){
      return new Promise((resolve, reject) => {
        axios.delete(`/phieuxemnha/DeletePhieuXemNha?phieuXemNhaId=${phieuXemNhaId}`)
          .then(res => resolve(res))
          .catch(error => reject(error))
      })
    },
  },
})