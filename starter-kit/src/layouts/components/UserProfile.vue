<script setup>
import { onMounted } from 'vue'

const logout = () => {
  localStorage.removeItem('accessToken')
  localStorage.removeItem('refreshToken')
  localStorage.removeItem('userInfo')
}

const userInfo = ref({
  
})

onMounted(()=>{
  userInfo.value = JSON.parse(localStorage.getItem('userInfo'))
})
</script>

<template>
  <VBadge
    dot
    location="bottom right"
    offset-x="3"
    offset-y="3"
    bordered
    color="success"
  >
    <VAvatar
      class="cursor-pointer"
      color="primary"
      variant="tonal"
    >
      <VIcon>mdi-user</VIcon>

      <!-- SECTION Menu -->
      <VMenu
        activator="parent"
        width="230"
        location="bottom end"
        offset="14px"
      >
        <VList>
          <!-- 👉 User Avatar & Name -->
          <VListItem>
            <template #prepend>
              <VListItemAction start>
                <VBadge
                  dot
                  location="bottom right"
                  offset-x="3"
                  offset-y="3"
                  color="success"
                >
                  <VAvatar
                    color="primary"
                    variant="tonal"
                  >
                    <VIcon>mdi-user</VIcon>
                  </VAvatar>
                </VBadge>
              </VListItemAction>
            </template>

            <VListItemTitle class="font-weight-semibold">
              {{ userInfo.UserName }}
            </VListItemTitle>
            <VListItemSubtitle>{{ userInfo.email }}</VListItemSubtitle>
          </VListItem>

          <!-- Divider -->
          <VDivider class="my-2" />

          <VListItem to="/account">
            <template #prepend>
              <VIcon
                class="me-2"
                icon="mdi-account"
                size="22"
              />
            </template>

            <VListItemTitle>
              Thông tin tài khoản
            </VListItemTitle>
          </VListItem>

          <!-- Divider -->
          <VDivider class="my-2" />

          <VListItem to="/change-password">
            <template #prepend>
              <VIcon
                class="me-2"
                icon="mdi-key"
                size="22"
              />
            </template>

            <VListItemTitle>
              Thay đổi mật khẩu
            </VListItemTitle>
          </VListItem>


          <VDivider class="my-2" />

          <!-- 👉 Logout -->
          <VListItem to="/login">
            <template #prepend>
              <VIcon
                class="me-2"
                icon="tabler-logout"
                size="22"
              />
            </template>

            <VListItemTitle @click="logout">
              Đăng xuất
            </VListItemTitle>
          </VListItem>
        </VList>
      </VMenu>
      <!-- !SECTION -->
    </VAvatar>
  </VBadge>
</template>
