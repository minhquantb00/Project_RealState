import axios from '@axios'
import { defineStore } from 'pinia'

export const useProductStore = defineStore('useProductStore', {
  actions: {
    getAll(params){
      return new Promise((resolve, reject) => {
        axios.put(`/product/GetAll?pageNumber=${params.pageNumber}&pageSize=${params.pageSize}`, params)
          .then(res => resolve(res))
          .catch(error => reject(error))
      })
    },
    getProductSoldAndPrices(params){
      return new Promise((resolve, reject) => {
        axios.get('/product/GetProductSoldAndPrices', { ...params })
          .then(res => resolve(res))
          .catch(error => reject(error))
      })
    },
    addNewProduct(params){
      return new Promise((resolve, reject) => {
        axios.post('/product/AddNewProduct', { ...params })
          .then(res => resolve(res))
          .catch(error => reject(error))
      })
    },
    addImageProduct(productId, params){
      return new Promise((resolve, reject) => {
        axios.post(`/product/AddImageForProduct/${productId}`, { ...params }, {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        })
          .then(res => resolve(res))
          .catch(error => reject(error))
      })
    },
    updateImageProduct(params){
      return new Promise((resolve, reject) => {
        axios.put(`/product/UpdateImageProduct`, { ...params }, {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        })
          .then(res => resolve(res))
          .catch(error => reject(error))
      })
    },
    updateProduct(id, params){
      return new Promise((resolve, reject) => {
        axios.put(`/product/UpdateProduct?productId=${id}`, { ...params })
          .then(res => resolve(res))
          .catch(error => reject(error))
      })
    },
    deleteProduct(id){
      return new Promise((resolve, reject) => {
        axios.put(`/product/DeleteProduct?productId=${id}`)
          .then(res => resolve(res))
          .catch(error => reject(error))
      })
    },
  },
})