<template>
	<div>
		<ul>
			<li v-for="account in accounts" :key="account.id" class="mb-2">
				<account :account="account" :broker-id="brokerId" />
			</li>
			<router-link :to="`/addAccount/${brokerId}`" class="btn btn-primary"
				>Add Account</router-link
			>
		</ul>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Account from '@/component/account.vue';
import { AccountDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'AccountList',
	components: {
		Account,
	},
})
export default class AccountList extends Vue {
	@Prop()
	brokerId!: string;

	@Prop()
	accounts!: AccountDto[];
}
</script>
