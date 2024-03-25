<script setup>
import axios from '@axios'
import { onMounted, ref } from 'vue'

const userInfo = JSON.parse(localStorage.getItem('userInfo'))

const user = ref({})

const getUserInfo = async () => {
  const res = await axios.get(`/user/GetUserByEmail?email=${userInfo.email}`)

  user.value = res.data.data
}

onMounted(() => {
  getUserInfo()
})
</script>

<template>
  <VCard style="padding: 10px;">
    <VCardTitle>
      <h2><p>Thông tin tài khoản</p></h2>
    </VCardTitle>
    <div class="px-5">
      <VDivider />
    </div>
    <VCardText>
      <VRow>
        <VCol cols="2">
          Email
        </VCol>
        <VCol cols="10">
          <VTextField
            v-model="user.email"
            readonly
          />
        </VCol>
        <VCol cols="2">
          Họ tên
        </VCol>
        <VCol cols="10">
          <VTextField v-model="user.name" />
        </VCol>
        <VCol cols="2">
          Số điện thoại
        </VCol>
        <VCol cols="10">
          <VTextField v-model="user.phoneNumber" />
        </VCol>
        <VCol cols="2">
          Ngày sinh
        </VCol>
        <VCol cols="10">
          <AppDateTimePicker
            v-model="user.dateOfBirth"
            clearable
            placeholder="Ngày sinh"
          />
        </VCol>
      </VRow>
    </VCardText>
    <VCardActions>
      <VSpacer />
      <VBtn>Cập nhật</VBtn>
    </VCardActions>
  </VCard>
</template>
