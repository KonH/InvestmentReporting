<template>
	<h1>Register</h1>
	<div class="form-group">
		<label>
			Token:
			<input ref="token" type="text" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Username:
			<input ref="login" type="text" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Password:
			<input ref="password" type="password" class="form-control" />
		</label>
	</div>
	<button :onclick="onclick" class="btn btn-primary">Register</button>
	<router-link to="/login" class="btn btn-secondary ml-2">Login</router-link>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import { Ref } from 'vue-property-decorator';

@Options({
	name: 'RegisterView',
})
export default class Register extends Vue {
	@Ref('token')
	tokenInput!: HTMLInputElement;

	@Ref('login')
	loginInput!: HTMLInputElement;

	@Ref('password')
	passwordInput!: HTMLInputElement;

	async onclick() {
		const inviteResult = await Backend.tryFetch(
			Backend.invite().register.registerCreate({
				token: this.tokenInput.value,
				userName: this.loginInput.value,
				password: this.passwordInput.value,
			})
		);
		if (inviteResult?.ok) {
			await router.push('/');
		} else {
			alert('Register failed');
		}
	}
}
</script>
