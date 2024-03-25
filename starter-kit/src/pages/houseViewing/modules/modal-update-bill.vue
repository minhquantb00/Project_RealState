<script setup>
import AppSingleImage from '@/components/AppSingleImage.vue'
import useEmitter from '@/helper/useEmitter'
import { useBillStore } from '@/services/bill/useBillStore'
import { useProductStore } from '@/services/product-services/useProductStore'
import { ref } from 'vue'

const emit = defineEmits(['refreshData'])

// #region data
const dialog = ref(false)
const products = ref([])
const isUpdate = ref(false)

const houseViewingBill = ref({
})

const loading = ref(false)
const form = ref()
const emitter = useEmitter()
const billStore = useBillStore()
const productStore = useProductStore()
const userInfo = JSON.parse(localStorage.getItem('userInfo'))

const requireFieldRule = [
  value => {
    if(value) return true
    
    return 'This field is required'
  },
]

const phoneNumberRules = [
  value => {
    if (value) return true

    return 'Số điện thoại là bắt buộc!'
  },
  value => {
    if (/(03|05|07|08|09|01[2|6|8|9])+([0-9]{8})\b/.test(value)) return true

    return 'Số điện thoại không hợp lệ!'
  },
]

// #endregion          

// #region methods
onMounted(() => {
  getAllProduct()
})

const getAllProduct = async () => {
  const params = {
    pageNumber: 1,
    pageSize: -1,
  }

  products.value = (await productStore.getAll(params)).data.data
  products.value.forEach(x=>{
    x.title =`${x.hostName} (${x.hostPhoneNumber}) - ${x.address}`
  })
}

const openDialog = bill => {
  dialog.value = !dialog.value
  isUpdate.value = false
  if(bill && bill.id > 0){
    isUpdate.value = true
    houseViewingBill.value = { ...bill }
    houseViewingBill.value.img1 = bill.custumerIdImg1 ? [bill.custumerIdImg1] : []
    houseViewingBill.value.img2 = bill.custumerIdImg2 ? [bill.custumerIdImg2] : []
  }
  else{
    houseViewingBill.value = {}
    houseViewingBill.value.img1 = []
    houseViewingBill.value.img2 = []
  }
}

const updateBill = async () => {
  try {
    loading.value = true
    houseViewingBill.value.nhanVienId = userInfo.Id
    houseViewingBill.value.custumerId = userInfo.Id
    houseViewingBill.value.custumerIdImg1 = houseViewingBill.value.img1[0]
    houseViewingBill.value.custumerIdImg2 = houseViewingBill.value.img2[0]
    if(!houseViewingBill.value.id){
      await billStore.createBill(houseViewingBill.value.nhaId, houseViewingBill.value.nhanVienId, houseViewingBill.value)
      emitter.emit('showAlert', {
        type: 'success',
        content: 'Success!',
      })
    }
    else{
      houseViewingBill.value.productId = houseViewingBill.value.nhaId
      houseViewingBill.value.phieuXemNhaId = houseViewingBill.value.id

      const res =  await billStore.updateBill(houseViewingBill.value)
      if(res.data.status === 200) {
        emitter.emit('showAlert', {
          type: 'success',
          content: res.data.message,
        })
      }
      else {
        emitter.emit('showAlert', {
          type: 'error',
          content: res.data.message,
        })
      }
    }
    
    emit('refreshData')
    dialog.value = false
    loading.value = false
  } catch (error) {
    emitter.emit('showAlert', {
      type: 'error',
      content: 'Server error!',
    })
    loading.value = false
  }
}


defineExpose({
  openDialog,
})

// #endregion
</script>

<template>
  <VRow justify="center">
    <VDialog
      v-model="dialog"
      persistent
      width="1024"
    >
      <VCard>
        <VCardTitle>
          <div
            class="text-h5 pt-5"
            style="text-align: center;"
          >
            <h2>PHIẾU XEM NHÀ</h2>
          </div>
        </VCardTitle>
        <VCardText>
          <VContainer>
            <VForm ref="form">
              <VRow>
                <VCol cols="12">
                  <h3><i>Thông tin phiếu</i></h3>
                </VCol>
                <VCol
                  cols="7"
                  style="display: flex; align-items: center;"
                >
                  <div style="padding-top: 10px; padding-right: 10px; font-weight: 600;">
                    Họ tên khách hàng*:
                  </div>
                  <VTextField
                    v-model="houseViewingBill.custumerName"
                    variant="underlined"
                    :rules="requireFieldRule"
                    required
                  />
                </VCol>
                <VCol
                  cols="5"
                  style="display: flex; align-items: center;"
                >
                  <div style="padding-top: 10px; padding-right: 10px; font-weight: 600;">
                    Số điện thoại khách hàng*:
                  </div>
                  <VTextField
                    v-model="houseViewingBill.custumerPhoneNumber"
                    :rules="phoneNumberRules"
                    variant="underlined"
                  />
                </VCol>
                <VCol cols="12">
                  <div style="font-weight: 600;">
                    Mô tả
                  </div>
                  <VTextarea
                    v-model="houseViewingBill.desciption"
                    required
                    variant="underlined"
                  />
                </VCol>
              </VRow>
              <VRow v-if="houseViewingBill && houseViewingBill.id > 0">
                <VCol cols="12">
                  <h3><i>Thông tin nhà</i></h3>
                </VCol>
                <VCol
                  cols="7"
                  style="display: flex; align-items: center;"
                >
                  <div style="padding-top: 10px; padding-right: 10px; font-weight: 600;">
                    Họ tên chủ:
                  </div>
                  <VTextField
                    v-model="houseViewingBill.product.hostName"
                    variant="underlined"
                    readonly
                  />
                </VCol>
                <VCol
                  cols="5"
                  style="display: flex; align-items: center;"
                >
                  <div style="padding-top: 10px; padding-right: 10px; font-weight: 600;">
                    Số điện thoại chủ:
                  </div>
                  <VTextField
                    v-model="houseViewingBill.product.hostPhoneNumber"
                    variant="underlined"
                    readonly
                  />
                </VCol>
                <VCol
                  cols="12"
                  style="display: flex; align-items: center;"
                >
                  <div style="padding-top: 10px; padding-right: 10px; font-weight: 600;">
                    Địa chỉ:
                  </div>
                  <VTextField
                    v-model="houseViewingBill.product.address"
                    variant="underlined"
                    readonly
                  />
                </VCol>
                <VCol
                  cols="12"
                  style="display: flex; align-items: center;"
                >
                  <div style="padding-top: 10px; padding-right: 10px; font-weight: 600;">
                    Thời gian xây dựng:
                  </div>
                  <AppDateTimePicker
                    v-model="houseViewingBill.product.build"
                    style="width: 200px;"
                  />
                </VCol>
                <VCol
                  cols="12"
                  style="display: flex; align-items: center;"
                >
                  <div style="padding-top: 10px; padding-right: 10px; font-weight: 600;">
                    Giấy chứng nhận đất đai 1:
                  </div>
                  <VTextField
                    v-model="houseViewingBill.product.certificateOfLand1"
                    variant="underlined"
                    readonly
                  />
                </VCol>
                <VCol
                  cols="12"
                  style="display: flex; align-items: center;"
                >
                  <div style="padding-top: 10px; padding-right: 10px; font-weight: 600;">
                    Giấy chứng nhận đất đai 2:
                  </div>
                  <VTextField
                    v-model="houseViewingBill.product.certificateOfLand2"
                    variant="underlined"
                    readonly
                  />
                </VCol>
                <VCol
                  cols="12"
                  style="display: flex; align-items: center;"
                >
                  <div style="padding-top: 10px; padding-right: 10px; font-weight: 600;">
                    Giá bán:
                  </div>
                  <VTextField
                    v-model="houseViewingBill.product.giaBan"
                    variant="underlined"
                    readonly
                  />
                </VCol>
              </VRow>
              <VRow>
                <VCol cols="12">
                  <VLabel style="width: 100%; margin: 24px 0 8px; font-weight: 600;">
                    Ảnh khách hàng 1
                  </VLabel>
                  <AppSingleImage
                    v-model="houseViewingBill.img1"
                    :default-img-url="houseViewingBill.custumerIdImg1 ? houseViewingBill.custumerIdImg1 : ''"
                  />
                </VCol>
                <VCol cols="12">
                  <VLabel style="width: 100%; margin: 24px 0 8px; font-weight: 600;">
                    Ảnh khách hàng 2
                  </VLabel>
                  <AppSingleImage
                    v-model="houseViewingBill.img2"
                    :default-img-url="houseViewingBill.custumerIdImg2 ? houseViewingBill.custumerIdImg2 : ''"
                  />
                </VCol>
                <VCol
                  v-if="!houseViewingBill || !houseViewingBill.banThanhCong"
                  cols="
                      12"
                >
                  <VSelect
                    v-model="houseViewingBill.nhaId"
                    :items="products"
                    item-value="id"
                    item-title="title"
                    label="Chọn nhà"
                  />
                </VCol>
                <VCol
                  v-if="isUpdate"
                  cols="4"
                >
                  <VSwitch
                    v-model="houseViewingBill.banThanhCong"
                    label="Đã bán?"
                    :color="houseViewingBill.banThanhCong ? 'success' : ''"
                  />
                </VCol>
              </VRow>
            </VForm>
          </VContainer>
          <small>*các trường bắt buộc</small>
        </VCardText>
        <VCardActions>
          <VSpacer />
          <VBtn
            color="blue-darken-1"
            variant="text"
            @click="dialog = false"
          >
            Đóng
          </VBtn>
          <VBtn
            color="blue-darken-1"
            variant="text"
            :loading="loading"
            @click="updateBill"
          >
            Lưu
          </VBtn>
        </VCardActions>
      </VCard>
    </VDialog>
  </VRow>
</template>
