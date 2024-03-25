<script setup>
import useEmitter from '@/helper/useEmitter'
import axios from '@axios'
import { ref } from 'vue'

const accountInfo = ref({})
const form = ref()
const emitter = useEmitter()
const isOldPasswordVisible = ref(false)
const isNewPasswordVisible = ref(false)
const isConfirmPasswordVisible = ref(false)

const changePassword = async () => {
  const { valid } = await form.value.validate()

  console.log(form)
  if(valid) {
    axios.put(`/auth/change-password`, { ...accountInfo.value })
      .then(res => {
        emitter.emit('showAlert', {
          type: 'success',
          content: res.data,
        })
      })
      .catch(error => {
        if(error.response) {
          emitter.emit('showAlert', {
            type: 'error',
            content: error.response.data,
          })
        }
      })
  }
}


const passwordRules = [
  value => {
    if (value) return true

    return 'Mật khẩu là bắt buộc!'
  },
  value => {
    if (/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/.test(value)) return true

    return 'Mật khẩu phải có ít nhất 8 ký tự, ít nhất 1 ký tự in hoa, ít nhất 1 ký tự in thường, ít nhất 1 số và 1 ký tự đặc biệt!'
  },
]
</script>

<template>
  <VCard style="padding: 10px;">
    <VCardTitle>
      <h2><p>Đổi mật khẩu</p></h2>
    </VCardTitle>
    <div class="px-5">
      <VDivider />
    </div>
    <VCardText>
      <VForm ref="form">
        <VRow>
          <VCol cols="2">
            Mật khẩu cũ
          </VCol>
          <VCol cols="10">
            <AppTextField
              v-model="accountInfo.oldPassword"
              label="Mật khẩu"
              :rules="passwordRules"
              :type="isOldPasswordVisible ? 'text' : 'password'"
              :append-inner-icon="isOldPasswordVisible ? 'tabler-eye-off' : 'tabler-eye'"
              @click:append-inner="isOldPasswordVisible = !isOldPasswordVisible"
            />
          </VCol>
          <VCol cols="2">
            Mật khẩu mới
          </VCol>
          <VCol cols="10">
            <AppTextField
              v-model="accountInfo.newPassword"
              label="Mật khẩu"
              :rules="passwordRules"
              :type="isNewPasswordVisible ? 'text' : 'password'"
              :append-inner-icon="isNewPasswordVisible ? 'tabler-eye-off' : 'tabler-eye'"
              @click:append-inner="isNewPasswordVisible = !isNewPasswordVisible"
            />
          </VCol>
          <VCol cols="2">
            Xác nhận mật khẩu mới
          </VCol>
          <VCol cols="10">
            <AppTextField
              v-model="accountInfo.confirmNewPassword"
              label="Mật khẩu"
              :rules="passwordRules"
              :type="isConfirmPasswordVisible ? 'text' : 'password'"
              :append-inner-icon="isConfirmPasswordVisible ? 'tabler-eye-off' : 'tabler-eye'"
              @click:append-inner="isConfirmPasswordVisible = !isConfirmPasswordVisible"
            />
          </VCol>
        </VRow>
      </VForm>
    </VCardText>
    <VCardActions>
      <VSpacer />
      <VBtn @click="changePassword">
        Đổi mật khẩu
      </VBtn>
    </VCardActions>
  </VCard>
</template>
