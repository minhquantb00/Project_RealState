<script setup>
import useEmitter from '@/helper/useEmitter'
import { useUserStore } from '@/services/user-services/useUserStore'
import { useGenerateImageVariant } from '@core/composable/useGenerateImageVariant'
import authV2LoginIllustrationBorderedDark from '@images/pages/auth-v2-login-illustration-bordered-dark.png'
import authV2LoginIllustrationBorderedLight from '@images/pages/auth-v2-login-illustration-bordered-light.png'
import authV2LoginIllustrationDark from '@images/pages/auth-v2-login-illustration-dark.png'
import authV2LoginIllustrationLight from '@images/pages/auth-v2-login-illustration-light.png'
import authV2MaskDark from '@images/pages/misc-mask-dark.png'
import authV2MaskLight from '@images/pages/misc-mask-light.png'
import { VNodeRenderer } from '@layouts/components/VNodeRenderer'
import { themeConfig } from '@themeConfig'
import { ref } from 'vue'
import { VForm } from 'vuetify/components/VForm'

// #region
const authThemeImg = useGenerateImageVariant(authV2LoginIllustrationLight, authV2LoginIllustrationDark, authV2LoginIllustrationBorderedLight, authV2LoginIllustrationBorderedDark, true)
const authThemeMask = useGenerateImageVariant(authV2MaskLight, authV2MaskDark)
const isPasswordVisible = ref(false)
const isPasswordConfirmVisible = ref(false)
const userStore = useUserStore()
const confirmCode = ref()
const isConfirm = ref(false)
const refVForm = ref()
const router = useRouter()
const emitter = useEmitter()

const user = ref({
  // username: 'admin',
  // email: 'quanghuynguyenba@gmail.com',
  // name: 'Admin',
  // phoneNumber: '0945123123',
  // password: 'Admin@123',
  // passwordConfirm: 'Admin@123',
})

var loading = ref(false)

//#region rules
const nameRules = [
  value => {
    if(value) return true
    
    return 'Họ tên là bắt buộc!'
  },
]

const emailRules = [
  value => {
    if (value) return true
    
    return 'Email là bắt buộc!'
  },
  value => {
    if (/.+@.+\..+/.test(value)) return true

    return 'Email không hợp lệ!'
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

const confirmPasswordRules = [
  value => {
    if (user.value.password === value) return true

    return 'Mật khẩu không trùng khớp!'
  },
]

// #endregion

// #endregion

// #region methods
const signUp = async () => {
  const { valid } = await refVForm.value.validate()

  if(valid){
    try {
      loading.value = true

      const res = await  userStore.addUser(user.value)

      console.log(res)

      if(res.data.status === 400) {
        emitter.emit('showAlert', {
          type: 'error',
          content: res.data.message,
        })
        loading.value =false
        
        return
      }

      emitter.emit('showAlert', {
        type: 'success',
        content: 'Success',
      })
      isConfirm.value=true
      loading.value =false
    } catch (error) {
      emitter.emit('showAlert', {
        type: 'error',
        content: 'Server error',
      })
      loading.value =false
    }
  }
  else {
    loading.value =false
    
    return false
  }
}

const onConfirm = async () => {
  const params = {
    confirmCode: confirmCode.value,
  }

  try {
    await userStore.confirmAddUser(params)
    emitter.emit('showAlert', {
      type: 'success',
      content: 'Success',
    })
    router.push('/')
  } catch (error) {
    emitter.emit('showAlert', {
      type: 'error',
      content: 'Server error',
    })
  }
}

// #endregion
</script>

<template>
  <VRow
    no-gutters
    class="auth-wrapper bg-surface"
  >
    <VCol
      lg="8"
      style="background-image: url(../public/real-estate-blur.jpg);"
    />

    <VCol
      cols="12"
      lg="4"
      class="auth-card-v2 d-flex align-center justify-center"
    >
      <VCard
        v-if="!isConfirm"
        flat
        :max-width="500"
        class="mt-12 mt-sm-0 pa-4"
      >
        <VCardText>
          <VNodeRenderer
            :nodes="themeConfig.app.logo"
            class="mb-6"
          />

          <h5 class="text-h5 mb-1">
            Hành trình bắt đầu tại đây 🚀
          </h5>

          <p class="mb-0">
            Làm ứng dụng quản lý của bạn dễ dàng và vui vẻ!
          </p>
        </VCardText>

        <VCardText>
          <VForm ref="refVForm">
            <VRow>
              <!-- username -->
              <VCol cols="12">
                <AppTextField
                  v-model="user.username"
                  label="Tài khoản"
                  type="text"
                  autofocus
                  :rules="nameRules"
                />
              </VCol>
                
              <!-- email -->
              <VCol cols="12">
                <AppTextField
                  v-model="user.email"
                  label="Email"
                  type="email"
                  :rules="emailRules"
                />
              </VCol>

              <!-- name -->
              <VCol cols="12">
                <AppTextField
                  v-model="user.name"
                  label="Họ tên"
                  type="text"
                />
              </VCol>

              <!-- name -->
              <VCol cols="12">
                <AppTextField
                  v-model="user.phoneNumber"
                  label="Số điện thoại"
                  type="text"
                  :rules="phoneNumberRules"
                />
              </VCol>

              <!-- password -->
              <VCol cols="12">
                <AppTextField
                  v-model="user.password"
                  label="Mật khẩu"
                  :type="isPasswordVisible ? 'text' : 'password'"
                  :append-inner-icon="isPasswordVisible ? 'tabler-eye-off' : 'tabler-eye'"
                  :rules="passwordRules"
                  @click:append-inner="isPasswordVisible = !isPasswordVisible"
                />
              </VCol>   

              <!-- password -->
              <VCol cols="12">
                <AppTextField
                  v-model="user.passwordConfirm"
                  label="Xác nhận mật khẩu"
                  :rules="confirmPasswordRules"
                  :type="isPasswordConfirmVisible ? 'text' : 'password'"
                  :append-inner-icon="isPasswordConfirmVisible ? 'tabler-eye-off' : 'tabler-eye'"
                  @click:append-inner="isPasswordConfirmVisible = !isPasswordConfirmVisible" 
                />
              </VCol>

              <!-- action -->
              <VCol cols="12">
                <VBtn
                  block
                  :loading="loading"
                  @click="signUp"
                >
                  Đăng ký
                </VBtn>
              </VCol>
              
              <!-- create account -->
              <VCol
                cols="12"
                class="text-center text-base"
              >
                <span>Bạn đã có tài khoản?</span>
                <RouterLink
                  class="text-primary ms-2"
                  to="/"
                >
                  Đăng nhập
                </RouterLink>
              </VCol>
            </VRow>
          </VForm>
        </VCardText>
      </VCard>
      <VCard v-else>
        <VCardText>
          <VNodeRenderer
            :nodes="themeConfig.app.logo"
            class="mb-6"
          />

          <h5 class="text-h5 mb-1">
            Mã xác nhận đã được gửi đến email của bạn 🚀
          </h5>

          <p class="mb-0">
            Vui lòng kiểm tra email và nhập mã xác nhận của bạn
          </p>
        </VCardText>
        <VCardText>
          <VForm
            @submit.prevent
            @submit="onConfirm"
          >
            <VRow>
              <VCol> <VTextField v-model="confirmCode" /></VCol>
              <VCol>
                <VBtn type="submit">
                  Submit
                </VBtn>
              </VCol>
            </VRow>
          </VForm>
        </VCardText>
      </VCard>
    </VCol>
  </VRow>
</template>

<style lang="scss">
@use "@core/scss/template/pages/page-auth.scss";
</style>

<route lang="yaml">
meta:
  layout: blank
</route>../../services/user-services/useUserStore