<script setup>
import AppSingleImage from '@/components/AppSingleImage.vue'
import useEmitter from '@/helper/useEmitter'
import { useProductStore } from '@/services/product-services/useProductStore'
import { onMounted, ref } from "vue"

const emit = defineEmits(['refreshData'])

// #region data
const dialog = ref(false)

const productInfo = ref({
  linkImg: "",
})

const productStore = useProductStore()
const loading = ref(false)
const form = ref()
const emitter = useEmitter()

const requireFieldRule = [
  value => {
    if(value) return true
    
    return 'Trường này là bắt buộc!'
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

onMounted(() => {
})


// #region methods
const openDialog = product => {
  dialog.value = !dialog.value
  productInfo.value = { ...product }
}

const updateProduct = async () => {

  const { valid } = await form.value.validate()

  loading.value = true
  if(valid){
    try {
      if(!productInfo.value.id){
        const res =  await productStore.addNewProduct(productInfo.value)

        const params = {
          LinkImg: productInfo.value.linkImg[0],
        }

        await productStore.addImageProduct(res.data.data.id, params)
      }
      else{
        await productStore.updateProduct(productInfo.value.id, productInfo.value)

        const params = {
          LinkImg: productInfo.value.linkImg ? productInfo.value.linkImg[0] : null,
        }

        if(productInfo.value.productImgDTOs && productInfo.value.productImgDTOs.length === 0)
          await productStore.addImageProduct(productInfo.value.id, params)
        else
        {
          params.ProductImgId = productInfo.value.productImgDTOs[0].id
          await productStore.updateImageProduct(params)
        }
      }
      emitter.emit('showAlert', {
        type: 'success',
        content: 'Success!',
      })
      dialog.value=false
      loading.value = false
      emit('refreshData')
    } catch (error) {
      console.log(error)
      emitter.emit('showAlert', {
        type: 'error',
        content: 'Server error',
      })
      loading.value = false
    }
  }
  else{
    loading.value = false
    
    return false
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
          <span class="text-h5">THÔNG TIN BẤT ĐỘNG SẢN</span>
        </VCardTitle>
        <VCardText>
          <VContainer>
            <VForm ref="form">
              <VRow>
                <VCol cols="8">
                  <VTextField
                    v-model="productInfo.hostName"
                    :rules="requireFieldRule"
                    label="Họ tên chủ*"
                    required
                  />
                </VCol>
                <VCol cols="4">
                  <VTextField
                    v-model="productInfo.hostPhoneNumber"
                    :rules="phoneNumberRules"
                    label="Số điện thoại chủ*"
                  />
                </VCol>
                <VCol cols="12">
                  <VTextField
                    v-model="productInfo.address"
                    label="Địa chỉ"
                    required
                  />
                </VCol>
                <VCol cols="6">
                  <AppDateTimePicker
                    v-model="productInfo.build"
                    label="Thời gian xây dựng"
                    placeholder="Thời gian xây dựng"
                  />
                </VCol>
                <VCol cols="6">
                  <AppDateTimePicker  
                    v-model="productInfo.batDauBan"
                    :rules="requireFieldRule"
                    label="Thời gian bắt đầu bán"
                    placeholder="Thời gian bắt đầu bán*"
                  />
                </VCol>
                <VCol cols="12">
                  <VTextField
                    v-model="productInfo.certificateOfLand1"
                    label="Giấy chứng nhận đất đai 1"
                    required
                  />
                </VCol>
                <VCol cols="12">
                  <VTextField
                    v-model="productInfo.certificateOfLand2"
                    label="Giấy chứng nhận đất đai 2"
                    required
                  />
                </VCol>
                <VCol cols="6">
                  <VTextField
                    v-model="productInfo.giaBan"
                    type="number"
                    :rules="requireFieldRule"
                    label="Giá trị*"
                    prefix="đ"
                  />
                </VCol>
                <VCol cols="6">
                  <VTextField
                    v-model="productInfo.phanTramChiaNV"
                    type="number"
                    :rules="requireFieldRule"
                    label="Tỷ lệ hoa hồng*"
                    prefix="%"
                  />
                </VCol>
                <VCol cols="12">
                  <VLabel style="width: 100%; margin: 24px 0 8px">
                    Ảnh
                  </VLabel>
                  <AppSingleImage
                    v-model="productInfo.linkImg"
                    :default-img-url="productInfo.productImgDTOs && productInfo.productImgDTOs.length > 0 ? productInfo.productImgDTOs[0].linkImg : ''"
                  />
                </VCol>
              </VRow>
            </VForm>
          </VContainer>
          <small style="color: rgb(243, 118, 118);">*trường bắt buộc</small>
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
            variant="flat"
            :loading="loading"
            @click="updateProduct"
          >
            Lưu
          </VBtn>
        </VCardActions>
      </VCard>
    </VDialog>
  </VRow>
</template>
